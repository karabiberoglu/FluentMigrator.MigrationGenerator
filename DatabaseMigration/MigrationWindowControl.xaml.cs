//------------------------------------------------------------------------------
// <copyright file="MigrationWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace DatabaseMigration
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using EnvDTE;
	using Microsoft.VisualStudio.Shell;

	/// <summary>
	/// Interaction logic for MigrationWindowControl.
	/// </summary>
	public partial class MigrationWindowControl : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MigrationWindowControl"/> class.
		/// </summary>
		public MigrationWindowControl()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Handles click on the button by displaying a message box.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
		[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			foreach (var item in GetEnvDTEProjectsInSolution())
			{
				projectNameCb.Items.Add(new ComboboxItem { Text = item.Name, Value = item.FileName });
			}
		}

		private static void FindProjectsIn(ProjectItem item, List<Project> results)
		{
			if (item.Object is Project)
			{
				var proj = (Project)item.Object;
				if (proj.Kind != Constants.vsProjectItemKindPhysicalFolder)
				{
					if (!string.IsNullOrWhiteSpace(proj.FileName))
						results.Add((Project)item.Object);
				}
				else
				{
					foreach (ProjectItem innerItem in proj.ProjectItems)
					{
						FindProjectsIn(innerItem, results);
					}
				}
			}
			if (item.ProjectItems != null)
			{
				foreach (ProjectItem innerItem in item.ProjectItems)
				{
					FindProjectsIn(innerItem, results);
				}
			}
		}

		private static void FindProjectsIn(UIHierarchyItem item, List<Project> results)
		{
			if (item.Object is Project)
			{
				var proj = (Project)item.Object;
				if (proj.Kind != Constants.vsProjectItemKindPhysicalFolder)
				{
					if (!string.IsNullOrWhiteSpace(proj.FileName))
						results.Add((Project)item.Object);
				}
				else
				{
					foreach (ProjectItem innerItem in proj.ProjectItems)
					{
						FindProjectsIn(innerItem, results);
					}
				}
			}
			foreach (UIHierarchyItem innerItem in item.UIHierarchyItems)
			{
				FindProjectsIn(innerItem, results);
			}
		}

		private static IEnumerable<Project> GetEnvDTEProjectsInSolution()
		{
			List<Project> ret = new List<Project>();
			EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
			UIHierarchy hierarchy = dte.ToolWindows.SolutionExplorer;
			foreach (UIHierarchyItem innerItem in hierarchy.UIHierarchyItems)
			{
				FindProjectsIn(innerItem, ret);
			}
			return ret;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string choice = null;
			if (CreateTable.IsChecked == true)
			{
				choice = "CreateTable";
			}
			else if (AlterTable.IsChecked == true)
			{
				choice = "AlterTable";
			}
			else if (InsertTable.IsChecked == true)
			{
				choice = "InsertTable";
			}
			else if (UpdateTable.IsChecked == true)
			{
				choice = "UpdateTable";
			}
			else if (DeleteTable.IsChecked == true)
			{
				choice = "DeleteTable";
			}

			if (choice != null)
			{
				string migrationNumber = DateTime.Now.ToString("yyyyMMddHHmm");

				string directory = (projectNameCb.SelectedItem as ComboboxItem).Value;
				string projectName = (projectNameCb.SelectedItem as ComboboxItem).Text;

				string migrationClassName = "Mig" + migrationNumber + "_" + choice + tableNameTb.Text;
				string directoryName = Path.Combine(directory.Substring(0, directory.LastIndexOf(@"\")), DateTime.Today.ToString("yyyy"), DateTime.Today.ToString("MM"));
				Directory.CreateDirectory(directoryName);

				string filePath = Path.Combine(directoryName, migrationClassName + ".cs");
				using (StreamWriter writer = new StreamWriter(filePath))
				{
					string namespaceStr = projectName + "._" + DateTime.Today.ToString("yyyy") + "._" + DateTime.Today.ToString("MM");
					string code = MigrationClassTemplate.Replace("<<Namespace>>", namespaceStr)
														.Replace("<<MigrationNumber>>", migrationNumber)
														.Replace("<<MigrationClassName>>", migrationClassName)
														.Replace("<<UpMethod>>", upTb.Text)
														.Replace("<<DownMethod>>", downTb.Text);
					writer.WriteLine(code);
				}

				OpenFile(filePath);
			}
		}

		private static void OpenFile(string filePath)
		{
			EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
			EnvDTE.Window item = dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindTextView);
			item.Activate();

			dte.ToolWindows.OutputWindow.ActivePane.OutputString($"{filePath} created successfully.{Environment.NewLine}");
		}

		private const string MigrationClassTemplate =
@"using FluentMigrator;

namespace <<Namespace>>
{
	[Migration(<<MigrationNumber>>)]
	public class <<MigrationClassName>> : Migration
	{
		public override void Up()
		{
			<<UpMethod>>
		}

		public override void Down()
		{
			<<DownMethod>>
		}
	}
}";
	}

	public class ComboboxItem
	{
		public string Text { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}
}