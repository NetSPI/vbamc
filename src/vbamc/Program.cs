﻿// Copyright 2022 Cisco Systems, Inc.
// Licensed under MIT-style license (see LICENSE.txt file).

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using DocumentFormat.OpenXml;
using McMaster.Extensions.CommandLineUtils;
using vbamc;

[Command(Name = "vbamc", Description = "Visual Basic for Applications macro compiler")]
public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Required]
    [Option("-m|--module")]
    public IEnumerable<string> Modules { get; } = Enumerable.Empty<string>();

    [Option("-c|--class")]
    public IEnumerable<string> Classes { get; } = Enumerable.Empty<string>();

    [Option("-n|--name", Description = "Project name")]
    public string ProjectName { get; } = "VBAProject";

    [Option("--company", Description = "Company name")]
    public string? CompanyName { get; }

    [Option("-f|--file", Description = "Target add-in file name")]
    public string FileName { get; } = "Presentation";

    [Option("-o|--output", Description = "Target build output path")]
    public string OutputPath { get; } = "bin";

    [Option("--intermediate", Description = "Intermediate path for build output")]
    public string IntermediatePath { get; } = "obj";

    private void OnExecute()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var wd = Directory.GetCurrentDirectory();
        var outputPath = Path.Combine(wd, this.OutputPath);
        var intermediatePath = Path.Combine(wd, this.IntermediatePath);

        var outputProjectName = @"vbaProject.bin";
        var outputFileName = this.FileName;


        var compiler = new VbaCompiler();

        compiler.ProjectId = Guid.NewGuid();
        compiler.ProjectName = this.ProjectName;
        compiler.CompanyName = this.CompanyName;

        // add modules
        foreach (var module in this.Modules)
        {
            var path = module;
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(wd, path);
            }

            compiler.AddModule(path);
        }

        // add classes
        foreach (var @class in this.Classes)
        {
            var path = @class;
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(wd, path);
            }

            compiler.AddClass(path);
        }

        var vbaProjectPath = compiler.CompileVbaProject(intermediatePath, outputProjectName);

        compiler.CompileMacroFile(outputPath, this.FileName, vbaProjectPath, PresentationDocumentType.MacroEnabledPresentation);
        compiler.CompileMacroFile(outputPath, this.FileName, vbaProjectPath, PresentationDocumentType.AddIn);
    }
}
