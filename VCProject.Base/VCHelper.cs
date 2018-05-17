﻿using EnvDTE;
using Microsoft.VisualStudio.VCProjectEngine;
using VCProjectUtils.Base;

#if VC14
namespace VCProjectUtils.VS14
#elif VC15
namespace VCProjectUtils.VS15
#endif
{
    public class VCHelper : IVCHelper
    {
        public bool IsVCProject(Project project)
        {
            return project?.Object is VCProject;
        }

        private VCFileConfiguration GetVCFileConfigForCompilation(Document document, out string reasonForFailure)
        {
            if (document == null)
            {
                reasonForFailure = "No document.";
                return null;
            }

            var vcProject = document.ProjectItem.ContainingProject?.Object as VCProject;
            if (vcProject == null)
            {
                reasonForFailure = "The given document does not belong to a VC++ Project.";
                return null;
            }

            VCFile vcFile = document.ProjectItem?.Object as VCFile;
            if (vcFile == null)
            {
                reasonForFailure = "The given document is not a VC++ file.";
                return null;
            }

            if (vcFile.FileType != eFileType.eFileTypeCppCode)
            {
                reasonForFailure = "The given document is not a compileable VC++ file.";
                return null;
            }

            IVCCollection fileConfigCollection = vcFile.FileConfigurations as IVCCollection;
            VCFileConfiguration fileConfig = fileConfigCollection?.Item(vcProject.ActiveConfiguration.Name) as VCFileConfiguration;
            if (fileConfig == null)
            {
                reasonForFailure = "Failed to retrieve file config from document.";
                return null;
            }

            reasonForFailure = "";
            return fileConfig;
        }

        public static VCCLCompilerTool GetCompilerTool(Project project, out string reasonForFailure)
        {
            VCProject vcProject = project?.Object as VCProject;
            if (vcProject == null)
            {
                reasonForFailure = "Failed to retrieve VCCLCompilerTool since project is not a VCProject.";
                return null;
            }
            VCConfiguration activeConfiguration = vcProject.ActiveConfiguration;
            var tools = activeConfiguration.Tools;
            VCCLCompilerTool compilerTool = null;
            foreach (var tool in activeConfiguration.Tools)
            {
                compilerTool = tool as VCCLCompilerTool;
                if (compilerTool != null)
                    break;
            }

            if (compilerTool == null)
            {
                reasonForFailure = "Couldn't file a VCCLCompilerTool in VC++ Project.";
                return null;
            }

            reasonForFailure = "";
            return compilerTool;
        }

        public static VCConfiguration GetConfigurationTool(Project project, out string reasonForFailure)
        {
            VCProject vcProject = project?.Object as VCProject;
            if (vcProject == null)
            {
                reasonForFailure = "Failed to retrieve VCConfiguration since project is not a VCProject.";
                return null;
            }
            VCConfiguration activeConfiguration = vcProject.ActiveConfiguration;
            var tools = activeConfiguration.Tools;
            VCConfiguration configurationTool = null;
            foreach (var tool in activeConfiguration.Tools)
            {
                configurationTool = tool as VCConfiguration;
                if (configurationTool != null)
                    break;
            }

            if (configurationTool == null)
            {
                reasonForFailure = "Couldn't file a VCConfiguration in VC++ Project.";
                return null;
            }

            reasonForFailure = "";
            return configurationTool;
        }

        public static VCLinkerTool GetLinkerTool(Project project, out string reasonForFailure)
        {
            VCProject vcProject = project?.Object as VCProject;
            if (vcProject == null)
            {
                reasonForFailure = "Failed to retrieve VCLinkerTool since project is not a VCProject.";
                return null;
            }
            VCConfiguration activeConfiguration = vcProject.ActiveConfiguration;
            var tools = activeConfiguration.Tools;
            VCLinkerTool linkerTool = null;
            foreach (var tool in activeConfiguration.Tools)
            {
                linkerTool = tool as VCLinkerTool;
                if (linkerTool != null)
                    break;
            }

            if (linkerTool == null)
            {
                reasonForFailure = "Couldn't file a VCLinkerTool in VC++ Project.";
                return null;
            }

            reasonForFailure = "";
            return linkerTool;
        }

        public bool IsCompilableFile(Document document, out string reasonForFailure)
        {
            return GetVCFileConfigForCompilation(document, out reasonForFailure) != null;
        }

        public void CompileSingleFile(Document document)
        {
            string reasonForFailure;
            var fileConfig = GetVCFileConfigForCompilation(document, out reasonForFailure);
            if(fileConfig != null)
            {
                fileConfig.Compile(true, false); // WaitOnBuild==true always fails.
            }
        }

        public string GetCompilerSetting_Includes(Project project, out string reasonForFailure)
        {
            VCCLCompilerTool compilerTool = GetCompilerTool(project, out reasonForFailure);
            return compilerTool?.FullIncludePath;
        }

        public void SetCompilerSetting_ShowIncludes(Project project, bool show, out string reasonForFailure)
        {
            VCCLCompilerTool compilerTool = GetCompilerTool(project, out reasonForFailure);
            if(compilerTool != null)
                compilerTool.ShowIncludes = show;
        }

        public bool? GetCompilerSetting_ShowIncludes(Project project, out string reasonForFailure)
        {
            VCCLCompilerTool compilerTool = GetCompilerTool(project, out reasonForFailure);
            return compilerTool?.ShowIncludes;
        }

        public string GetCompilerSetting_PreprocessorDefinitions(Project project, out string reasonForFailure)
        {
            VCCLCompilerTool compilerTool = GetCompilerTool(project, out reasonForFailure);            
            var defines = compilerTool?.PreprocessorDefinitions;

            //string reason2 = string.Empty;
            //VCConfiguration projectConfiguration = GetConfigurationTool(project, out reason2);
            //reasonForFailure += reason2;

            //if (projectConfiguration.CharacterSet == charSet.charSetUnicode)
            //{
                defines += ";_UNICODE";
            //}
            return defines;
        }

        public TargetMachineType? GetLinkerSetting_TargetMachine(EnvDTE.Project project, out string reasonForFailure)
        {
            var linkerTool = GetLinkerTool(project, out reasonForFailure);
            if (linkerTool == null)
                return null;
            else
                return (TargetMachineType)linkerTool.TargetMachine;
        }
    }
}
