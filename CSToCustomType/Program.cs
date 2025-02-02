using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CSToCustomType
{
    internal class Program
    {
        private static List<string> dependencyFolders = new List<string>();

        [STAThread]
        static void Main()
        {
            string assemblyPath = null;
            string outputPath = null;

            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                Description = "Select your Beat Saber folder"
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(Path.Combine(fbd.SelectedPath, "Beat Saber.exe")))
                {
                    MessageBox.Show("Invalid Beat Saber folder. Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string beatSaberPath = fbd.SelectedPath;
                dependencyFolders.Add(Path.Combine(beatSaberPath, "Plugins"));
                dependencyFolders.Add(Path.Combine(beatSaberPath, "Libs"));
                dependencyFolders.Add(Path.Combine(beatSaberPath, "Beat Saber_Data", "Managed"));
            }

            SelectDependencyFolders();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll",
                Title = "Select Assembly File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                assemblyPath = openFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(assemblyPath))
            {
                MessageBox.Show("No assembly selected. Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            fbd.Description = "Select Output Directory";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                outputPath = fbd.SelectedPath;
            }

            if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
            {
                MessageBox.Show("Invalid output directory. Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return ResolveAssembly(args.Name);
            };

            AssemblyParser parser = new AssemblyParser(assemblyPath, outputPath);
            Console.WriteLine("Done!");
            System.Diagnostics.Process.Start("explorer.exe", outputPath);
        }

        private static void SelectDependencyFolders()
        {
            do
            {
                DialogResult result = MessageBox.Show("Do you want to add an extra dependency folder?\n\nCommon folders such as Plugins, Libs, and Beat Saber_Data/Managed are already added", "Add Extra Dependencies", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    break;
                }

                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = "Select a folder that contains dependencies (DLLs)"
                };

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    if (!dependencyFolders.Contains(fbd.SelectedPath))
                    {
                        dependencyFolders.Add(fbd.SelectedPath);
                    }
                }
            } while (true);
        }

        private static Assembly ResolveAssembly(string assemblyName)
        {
            string assemblyFileName = new AssemblyName(assemblyName).Name + ".dll";
            foreach (string directory in dependencyFolders)
            {
                string assemblyPath = Path.Combine(directory, assemblyFileName);
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            return null;
        }
    }
}
