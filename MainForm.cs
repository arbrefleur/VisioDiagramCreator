﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisioDiagramCreator.ExcelHelpers;
using VisioDiagramCreator.Extensions;
using VisioDiagramCreator.Models;
using VisioDiagramCreator.Visio;

namespace VisioDiagramCreator
{
	public partial class MainForm : Form
	{
		DiagramData diagramData = null;
		Boolean _bBuildVisioFromExcelDataFile = true;

		VisioHelper visHlp = new VisioHelper();

		static string baseWorkingDir = @"C:\Omnicell_Diagram_Creator";
		static string excelDataPath = baseWorkingDir + @"\ExcelData\";
		static string scriptDataPath = baseWorkingDir + @"\data\ScriptData\";
		static string visioFilesPath = baseWorkingDir + @"\VisioFiles\";

		public MainForm()
		{
			InitializeComponent();

			diagramData = new DiagramData();
			//diagramData.visioTemplateFilePath = @"C:\Omnicell_Diagram_Creator\data\Templates\OC_ArchitectDiagramTemplate.vstx";
			//diagramData.StencilFilePath = @"C:\Omnicell_Diagram_Creator\data\Stencils\OC_ArchitectStencils.vssx";
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			///////////////////////////////////////////////////////////////////////
			// this section is for Building the excel data file from a Visio file
			// use todays date as part of the file name
			tb_buildExcelFileName.Text = String.Format("ExcelDataFile_{0}.xlxs", DateTime.Now.ToString("MMddyyyy"));
			tb_buildExcelPath.Text = excelDataPath;
			//////////////////////////////////////////////////////////////////////

			_bBuildVisioFromExcelDataFile = true;
			rb_buildExcelFileFromVisio.Enabled = false;		// underconstruction so don't enable this option

#if DEBUG
			rb_buildExcelFileFromVisio.Enabled = true;
#endif
			btn_Submit.Enabled = false;

			btn_SetExcelPath.Enabled = false;
			btn_VisioFileToRead.Enabled = false;

			rb_buildFromExcelFile.Checked = true;
			tb_buildExcelFileName.Enabled = false;
			tb_buildExcelPath.Enabled = false;
			tb_buildVisioFilePath.Enabled = false;
		}

		private void btn_Quit_Click(object sender, EventArgs e)
		{
			visHlp.VisioForceCloseAll();
			this.Close();
		}

		private void btn_Submit_Click(object sender, EventArgs e)
		{
			// parse the data file and draw the visio diagram
			try
			{
				if (_bBuildVisioFromExcelDataFile)
				{
					// build visio file form data file
					ConsoleOut.writeLine(String.Format("MainForm - Build Visio file from an excel data file:{0}", tb_excelDataFile.Text));
					diagramData = new ProcessExcelDataFile().ParseData(tb_excelDataFile.Text.Trim(), diagramData);
					if (diagramData == null)
					{
						MessageBox.Show("MainForm - ERROR: _parseData returned null");
						visHlp.VisioForceCloseAll();
						this.Close();
					}

					if (!visHlp.DrawAllShapes(diagramData, VisioVariables.ShowDiagram.Show))
					{
						// build the shape connection map to be used to establish connections between shapes on the diagrams
						diagramData.ShapeConnectionsMap = new ProcessVisioShapeConnections().BuildShapeConnections(diagramData);

						// Lets make the connections 
						bool bAns = visHlp.ConnectShapes(diagramData);

						// we need to close everything
#if !DEBUG
						visHlp.VisioForceCloseAll();
#endif
					}
				}
				else
				{
					// for testing to view all the stencils in the document
					//visHlp.ListDocumentStencils(diagramData, VisioVariables.ShowDiagram.Show);

					// buid data file from existing Visio file
					ConsoleOut.writeLine("build excel data file from a Visio file");

					Dictionary<int, ShapeInformation> shapesMap = new ProcessVisioDiagramShapes().GetAllShapesProperties(tb_buildVisioFilePath.Text.Trim(), VisioVariables.ShowDiagram.Show);
					foreach (var allShp in shapesMap)
					{
						int nKey = allShp.Key;
						ShapeInformation shpInf = allShp.Value;
						ConsoleOut.writeLine(string.Format("MainForm - ID:{0}; UniqueKey:{1}; Image:{2}, ConnectToID:{3}; ConnectTo:{4}; ToLabel:{5}; ConnectFromID:{6}; ConnectFrom:{7}; FromLabel:{8}", shpInf.ID, shpInf.UniqueKey, shpInf.StencilImage, shpInf.ConnectToID, shpInf.ConnectTo, shpInf.ToLineLabel, shpInf.ConnectFromID, shpInf.ConnectFrom, shpInf.FromLineLabel));
					}
					if (shapesMap != null)
					{
						CreateExcelDataFile createExcelDataFile = new CreateExcelDataFile();
						if (createExcelDataFile.PopulateExcelDataFile(shapesMap, this.tb_excelDataFile.Text))
						{
							MessageBox.Show(String.Format("Error::MainForm - Creating excel data file:{0}", this.tb_excelDataFile.Text));
						}

					}
					// we are dont so we can close the visio document(s)
					visHlp.VisioForceCloseAll();
				}
				diagramData.Reset();
			}
			catch( IOException ioe)
			{
				MessageBox.Show(string.Format("Exception::MainForm - {0}",ioe.Message), "Warning File Access Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Exception::MainForm - {0}\n{1}", ex.Message,ex.StackTrace), "Exception");
			}
		}

		private void rb_buildFromDataFile_CheckedChanged(object sender, EventArgs e)
		{
			if(rb_buildFromExcelFile.Checked)
			{
				_bBuildVisioFromExcelDataFile = true;
				tb_excelDataFile.Enabled = true;

				rb_buildExcelFileFromVisio.Checked = false;
				tb_buildExcelFileName.Enabled = false;

				btn_readExcelfile.Enabled = true;

				btn_SetExcelPath.Enabled = false;
				btn_VisioFileToRead.Enabled = false;
			}
		}

		private void rb_buildDataFileFromVisio_CheckedChanged(object sender, EventArgs e)
		{
			if (rb_buildExcelFileFromVisio.Checked)
			{
				_bBuildVisioFromExcelDataFile = false;

				tb_buildExcelFileName.Enabled = true;

				tb_excelDataFile.Enabled = false; 
				rb_buildFromExcelFile.Checked = false;

				btn_SetExcelPath.Enabled = true;
				btn_VisioFileToRead.Enabled = true;

				btn_readExcelfile.Enabled = false;
			}
		}

		private void tb_buildExcelFileName_TextChanged(object sender, EventArgs e)
		{
			// check if a valid file name
			if (IsValidFileName(tb_buildExcelFileName.Text) && IsFormValidated(_bBuildVisioFromExcelDataFile))
			{
				btn_Submit.Enabled = true;
			}
			else
			{
				btn_Submit.Enabled = false;
			}
		}

		private void btn_openExcelPath_Click(object sender, EventArgs e)
		{
			string folder = string.Empty;
			folder = FileExtension.getFolder(excelDataPath, "Select the Excel output path");
			if (string.IsNullOrEmpty(folder))
			{
				// Cancel was pressed.  filePath will be empty
				ConsoleOut.writeLine("Cancel button pressed.  No folder selected");
			}
			else
			{
				// this will contain the folder path
				excelDataPath = folder;
				tb_buildExcelPath.Text = folder;

				// check if valid is so enable submit button
				if (IsValidFileName(tb_buildExcelFileName.Text) && IsFormValidated(_bBuildVisioFromExcelDataFile))
				{
					btn_Submit.Enabled = true;
				}
				else
				{
					btn_Submit.Enabled = false;
				}
			}
		}

		private void btn_VisioFileToRead_Click(object sender, EventArgs e)
		{
			string filePath = string.Empty;
			filePath = FileExtension.getFilePath(visioFilesPath, "vsdx files (*.vsdx)|*.vsdx", "Select a Visio file to process into an Excel data file");
			if (string.IsNullOrEmpty(filePath))
			{
				// Cancel was pressed.  filePath will be empty
				ConsoleOut.writeLine("Cancel button pressed.  No file was selected");
			}
			else
			{
				tb_buildVisioFilePath.Text = filePath;

				// check if valid is so enable submit button
				if (IsValidFileName(tb_buildExcelFileName.Text) && IsFormValidated(_bBuildVisioFromExcelDataFile))
				{
					btn_Submit.Enabled = true;
				}
				else
				{
					btn_Submit.Enabled = false;
				}
			}
		}

		private void btn_readExcelfile_Click(object sender, EventArgs e)
		{
			string filePath = string.Empty;
			filePath = FileExtension.getFilePath(scriptDataPath, "vsdx files (*.xls)|*.xlsx", "Select the Excel data file to build a Visio diagram");
			if (string.IsNullOrEmpty(filePath))
			{
				// Cancel was pressed.  filePath will be empty
				ConsoleOut.writeLine("Cancel button pressed.  No file was selected");
			}
			else
			{
				//Get the path of specified file
				tb_excelDataFile.Text = filePath;

				// check if valid is so enable submit button
				if (IsFormValidated(_bBuildVisioFromExcelDataFile))
				{
					btn_Submit.Enabled = true;
				}
				else
				{
					btn_Submit.Enabled = false;
				}
			}
		}


		/// <summary>
		/// validate_field
		/// validate the form fields based on the buildMode checkbox optoin
		/// </summary>
		/// <param name="buildMode">true (Build Visio diagram from Excel data file)  false (Build Excel data file from a Visio diagram)</param>
		/// <returns>true - Valid / false - Not Valid</returns>
		private bool IsFormValidated(bool buildMode)
		{
			//_bBuildVisioFromExcelDataFile
			if (buildMode == true)
			{
				// we are using the Excel data file to build the visio diagram
				if (string.IsNullOrEmpty(tb_excelDataFile.Text))
				{
					return false;
				}
			}
			else
			{
				// we are building an excel data file from a Visio diagram file
				// check required fields and paths
				if (string.IsNullOrEmpty(tb_buildExcelPath.Text))
				{
					return false;
				}
				if (string.IsNullOrEmpty(tb_buildExcelFileName.Text))
				{
					return false;
				}
				if (string.IsNullOrEmpty(tb_buildVisioFilePath.Text))
				{
					return false;
				}			
			}
			return true;
		}

		/// <summary>
		/// IsValidFileName
		/// validate the filename is a valid file name
		/// </summary>
		/// <param name="testName">filename</param>
		/// <returns>true - Valid / false - Not valid</returns>
		bool IsValidFileName(string testName)
		{
			Regex containsABadCharacter = new Regex("[" + Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
			if (containsABadCharacter.IsMatch(testName)) 
			{ 
				return false; 
			}
			// other checks for UNC, drive-path format, etc

			return true;
		}

	}
}
