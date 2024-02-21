﻿using System;
using System.Threading;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Shockky.SourceGeneration.Models;
using Shockky.SourceGeneration.Helpers;
using Shockky.SourceGeneration.Extensions;
using Shockky.SourceGeneration.Diagnostics;

namespace Shockky.SourceGeneration;

[Generator(LanguageNames.CSharp)]
public sealed class InstructionGenerator : IIncrementalGenerator
{
    private const string OPAttributeFullName = "Shockky.Lingo.Instructions.OPAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all the annotated instruction definitions
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<InstructionInfo?> Info)> instructionInfoResults =
            context.SyntaxProvider.ForAttributeWithMetadataName(OPAttributeFullName,
                predicate: static (node, token) => node is EnumMemberDeclarationSyntax,
                transform: static (context, token) =>
                {
                    var enumMemberDeclaration = (EnumMemberDeclarationSyntax)context.TargetNode;
                    var fieldSymbol = (IFieldSymbol)context.TargetSymbol;

                    var hierarchyInfo = HierarchyInfo.From(fieldSymbol.ContainingType);

                    _ = TryGetInfo(
                        enumMemberDeclaration,
                        fieldSymbol,
                        context.SemanticModel,
                        token,
                        out InstructionInfo? instructionInfo,
                        out ImmutableArray<DiagnosticInfo> diagnostics);

                    return (hierarchyInfo, new Result<InstructionInfo?>(instructionInfo, diagnostics));
                })
            .WithTrackingName(nameof(instructionInfoResults));

        context.ReportDiagnostics(instructionInfoResults.Select(static (item, _) => item.Info.Errors));

        // Generate instruction definitions
        context.RegisterSourceOutput(instructionInfoResults, static (context, item) =>
        {
            using IndentedTextWriter writer = new();
            WriteInstructionSyntax(writer, item.Hierarchy, item.Info.Value!);

            context.AddSource($"{item.Hierarchy.Namespace}.{item.Info.Value!.Name}.g.cs", writer.ToString());
        });

        // TODO: Gather all instructions and generate factory method
    }

    internal static bool TryGetInfo(
        EnumMemberDeclarationSyntax enumMemberSyntax, 
        IFieldSymbol fieldSymbol, 
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        [NotNullWhen(true)] out InstructionInfo? instructionInfo,
        out ImmutableArray<DiagnosticInfo> diagnostics)
    {
        // TODO: More sanity checks
        using ImmutableArrayBuilder<DiagnosticInfo> diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

        // Get ImmediateKind from the attribute if given.
        ImmediateKind immediateKind = ImmediateKind.None;

        if (fieldSymbol.TryGetAttributeWithFullyQualifiedMetadataName(OPAttributeFullName, out AttributeData? attributeData) &&
            attributeData.ConstructorArguments is [{ Value: int immediateKindValue }])
        {
            immediateKind = (ImmediateKind)immediateKindValue;
        }

        // Get instruction OP value from the assigned constant expression
        if (enumMemberSyntax.EqualsValue is null)
        {
            diagnosticBuilder.Add(
                DiagnosticDescriptors.MissingExplicitValueOnOPAttributeAnnotatedEnumMember,
                fieldSymbol,
                fieldSymbol.ContainingType, fieldSymbol.Name);
            
            instructionInfo = null;
            diagnostics = diagnosticBuilder.ToImmutable();

            return false;
        }

        var equalsValueExpression = enumMemberSyntax.EqualsValue.Value;
        if (semanticModel.GetOperation(equalsValueExpression) is not IOperation equalsValueOperation)
        {
            throw new ArgumentException("Failed to retrieve an operation for the equals expression");
        }
        
        var enumValueConstant = TypedConstantInfo.From(
            equalsValueOperation, semanticModel, equalsValueExpression, cancellationToken);

        instructionInfo = new InstructionInfo(fieldSymbol.Name, enumValueConstant, immediateKind);
        diagnostics = diagnosticBuilder.ToImmutable();

        return true;
    }

    internal static void WriteInstructionSyntax(IndentedTextWriter writer, 
        HierarchyInfo hierarchy,
        InstructionInfo instruction)
    {
        writer.WriteLine($"namespace {hierarchy.Namespace};");
        writer.WriteLine();

        writer.WriteGeneratedAttributes(nameof(InstructionGenerator));
        writer.WriteLine($"public sealed class {instruction.Name} : IInstruction");

        using var block = writer.WriteBlock();
        {
            bool hasImmediate = instruction.ImmediateKind is not ImmediateKind.None;

            // Offer cached instance for instruction with no immediates
            writer.WriteLineIf(!hasImmediate, $"public static readonly {instruction.Name} Default = new();");
            writer.WriteLineIf(!hasImmediate);


            writer.WriteLine($"public OPCode OP => OPCode.{instruction.Name};");
            writer.WriteLine();
            writer.WriteLine("public int Immediate { get; set; }");
        }
    }
}
