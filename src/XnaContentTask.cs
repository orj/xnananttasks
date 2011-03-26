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
using System;
using System.Security;
using System.Xml;
using Microsoft.Win32;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.Examples.Tasks 
{
    
    /// <summary>
    /// NAnt task for building XNA Content.
    /// </summary>
    [TaskName("xnacontent")]
    public class XnaContentTask : Task 
    {
        private string m_intermediateDirectory;
        private string m_outputDirectory;
        private string m_targetPlatform;

        private FileSet m_assembliesFileset = new FileSet();
        private AssetFileSet[] m_assets;
        private XnaContentProject m_project;
        private string m_baseDirectory;

        [TaskAttribute("intermediatedir", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string IntermediateDirectory 
        {
            get { return m_intermediateDirectory; }
            set { m_intermediateDirectory = value; }
        }

        [TaskAttribute("outputdir", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string OutputDirectory
        {
            get { return m_outputDirectory; }
            set { m_outputDirectory = value; }
        }

        [TaskAttribute("targetplatform", Required=true) ]
        [StringValidator(AllowEmpty=false)]
        public string TargetPlatform
        {
            get { return m_targetPlatform; }
            set { m_targetPlatform = value; }
        }

        [TaskAttribute("basedir", Required=true)]
        [StringValidator(AllowEmpty = false)]
        public string BaseDirectory
        {
            get { return m_baseDirectory; }
            set { m_baseDirectory = value; }
        }

        [BuildElement("assemblies")]
        public FileSet AssemblyFileSet 
        {
            get { return m_assembliesFileset; }
            set { m_assembliesFileset = value; }
        }

        [BuildElementArray("assets")]
        public AssetFileSet[] Assets
        {
            get { return m_assets; }
            set { m_assets = value; }
        }

        protected override void InitializeTask(XmlNode taskNode)
        {
            base.InitializeTask(taskNode);

            // InitDefaultOptions();
            m_project =
                new XnaContentProject(this, Project.Frameworks["net-2.0"].FrameworkDirectory.FullName,
                                      GetXnaInstallPath());
        }

        private static string GetXnaInstallPath()
        {
            string xnaInstallPath;
            try
            {
                object keyValue = Registry.LocalMachine.OpenSubKey("Software")
                    .OpenSubKey("Microsoft")
                    .OpenSubKey(".NETFramework")
                    .OpenSubKey("v2.0.50727")
                    .OpenSubKey("AssemblyFoldersEx")
                    .OpenSubKey("XNA Framework for x86").GetValue("");
                
                xnaInstallPath = keyValue.ToString();
            }
            catch (SecurityException securityException)
            {
                throw new BuildException("Could not access registry.", securityException);
            }
            catch (ArgumentException argumentException)
            {
                throw new BuildException("Could not find XNA installation directory. Check that XNA is installed.", argumentException);
            }
            return xnaInstallPath;
        }

        protected override void ExecuteTask() 
        {
            foreach (AssetFileSet fileSet in m_assets)
            {
                foreach (string filename in fileSet.FileNames)
                {
                    m_project.AddContent(filename, fileSet.Importer, fileSet.Processor);
                }
            }

            m_project.Build(BaseDirectory, IntermediateDirectory, OutputDirectory, TargetPlatform);
        }
    }

    [ElementName("assets")]
    public class AssetFileSet : FileSet
    {
        private string m_importer;
        private string m_processor;

        [TaskAttribute("importer", Required=true)]
        [StringValidator(AllowEmpty = false)]
        public string Importer
        {
            get { return m_importer; }
            set { m_importer = value; }
        }

        [TaskAttribute("processor", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Processor
        {
            get { return m_processor; }
            set { m_processor = value; }
        }
    }
}
