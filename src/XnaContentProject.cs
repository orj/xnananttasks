/* Copyright (c) Oliver Jones
 * This work is licensed under a Creative Commons Attribution 3.0 License.
 * http://creativecommons.org/licenses/by/3.0/
 *
 * Thanks to the guys a XNADev.ru for releasing the code to their XNA Content 
 * Builder app & framework.  I've liberally pinched code from there to build
 * this NAnt task.
 * 
 * Created by: Oliver Jones
 * Created: Sunday, 1 April 2007
 * SVN: $Id$
 */

using System.IO;
using Microsoft.Build.BuildEngine;
using NAnt.Core;
using Project=Microsoft.Build.BuildEngine.Project;
using Target=Microsoft.Build.BuildEngine.Target;

namespace NAnt.Examples.Tasks
{
    /// <summary>A wrapper around an MSBuild Project that creates and builds XNA Content.</summary>    
    public class XnaContentProject
    {
        private Project m_project;
        private Engine m_engine;
        private BuildItemGroup m_pipelineGroup;
        private BuildItemGroup m_contentGroup;
        private Target m_contentTarget;
        private Target m_xnaTarget;

        public XnaContentProject(Task task, string msBuildPath, string xnaInstallPath)
        {
            m_engine = new Engine(msBuildPath);
            m_engine.RegisterLogger(new XnaContentLogger(task));
            m_project = new Project(m_engine);

            m_project.AddNewUsingTaskFromAssemblyName("BuildContent", "Microsoft.Xna.Framework.Content.Pipeline, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6d5c3888ef60e27d");
            m_project.AddNewUsingTaskFromAssemblyName("BuildXact", "Microsoft.Xna.Framework.Content.Pipeline, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6d5c3888ef60e27d");

            // Add our Content Pipeline Assemblies
            m_pipelineGroup = m_project.AddNewItemGroup();
            m_contentGroup = m_project.AddNewItemGroup();

            m_contentTarget = m_project.Targets.AddNewTarget("_BuildXNAContentLists");
            
            // Add our Build target
            m_xnaTarget = m_project.Targets.AddNewTarget("Build");
            m_xnaTarget.DependsOnTargets = "_BuildXNAContentLists";

            // Add Default Pipeline Assemblies.
            AddPilepineAssembly(xnaInstallPath + "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll");
            AddPilepineAssembly(xnaInstallPath + "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter.dll");
            AddPilepineAssembly(xnaInstallPath + "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll");
            AddPilepineAssembly(xnaInstallPath + "Microsoft.Xna.Framework.Content.Pipeline.XImporter.dll");
        }

        public void AddPilepineAssembly(string filename)
        {
            m_pipelineGroup.AddNewItem("PipelineAssembly", filename);
        }

        public void AddContent(string filename, string importer, string processor)
        {
            BuildItem content = m_contentGroup.AddNewItem("Content", filename);

            content.SetMetadata("Importer", importer);
            content.SetMetadata("Processor", processor);
            content.SetMetadata("Name", Path.GetFileNameWithoutExtension(filename));
        }

        public void Build(string rootDirectory, string intermediateDirectory, string outputDirectory, string targetPlatform)
        {
            AddContentBuildTask(intermediateDirectory, outputDirectory, rootDirectory, targetPlatform);
            AddXActBuildTask(intermediateDirectory, outputDirectory, rootDirectory, targetPlatform);

            m_project.Build("Build");
        }

        private void AddContentBuildTask(string intermediateDirectory, string outputDirectory, string rootDirectory, string targetPlatform)
        {
            BuildTask contentTask = m_contentTarget.AddNewTask("CreateItem");
            contentTask.SetParameterValue("Include", "@(Content)");
            contentTask.Condition = "'%(Content.Importer)' != 'XactImporter'";
            contentTask.AddOutputItem("Include", "XNAContent");

            BuildTask bt = m_xnaTarget.AddNewTask("BuildContent");
            bt.SetParameterValue("SourceAssets", "@(XNAContent)");
            bt.SetParameterValue("PipelineAssemblies", "@(PipelineAssembly)");
            bt.SetParameterValue("IntermediateDirectory", intermediateDirectory);
            bt.SetParameterValue("OutputDirectory", outputDirectory);
            bt.SetParameterValue("RootDirectory", rootDirectory);
            bt.SetParameterValue("TargetPlatform", targetPlatform);
        }

        private void AddXActBuildTask(string intermediateDirectory, string outputDirectory, string rootDirectory, string targetPlatform)
        {
            BuildTask xactTask = m_contentTarget.AddNewTask("CreateItem");
            xactTask.SetParameterValue("Include", "@(Content)");
            xactTask.Condition = "'%(Content.Importer)' == 'XactImporter'";
            xactTask.AddOutputItem("Include", "XACTContent");

            BuildTask bt = m_xnaTarget.AddNewTask("BuildXact");
            bt.SetParameterValue("XactProjects", "@(XACTContent)");
            bt.SetParameterValue("IntermediateDirectory", intermediateDirectory);
            bt.SetParameterValue("OutputDirectory", outputDirectory);
            bt.SetParameterValue("RootDirectory", rootDirectory);
            bt.SetParameterValue("TargetPlatform", targetPlatform);
            bt.SetParameterValue("XnaFrameworkVersion", "v1.0");
        }
    }
}