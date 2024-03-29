﻿using DiscreteEventSimulationLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Charting = System.Windows.Forms.DataVisualization.Charting;

namespace ProductionFlowSimulation
{
    public partial class MainForm : Form
    {
        Rectangle theRectangle = new Rectangle();

        Point startPoint;
        Point endPoint;
        DESElement selectedElement = null;
        DiscreteEventSimulationModel theModel = new DiscreteEventSimulationModel();
        List<DESElement> allElements = new List<DESElement>();
        ContinuousRandomGeneratorType currentDistribution = ContinuousRandomGeneratorType.None;

        bool isDrag;

        public MainForm()
        {
            InitializeComponent();
            InitializeChart();
            InitializeElementPropertyEditor();
            this.CenterToScreen();
            WindowState = FormWindowState.Maximized;
            MinimumSize = MaximumSize;
        }

        #region Initialization
        private void panelMain_Scroll(object sender, ScrollEventArgs e)
        {
            panelMain.Refresh();
        }

        private void InitializeElementPropertyEditor()
        {
            ppgObject.SelectedObject = theModel;
            cbbObject.Items.Add(theModel.Name);
            cbbObject.SelectedIndex = 0;
            DESCollectionElementEditor.DESElementAddedEvent += DESCollectionElementEditor_DESElementAdded;
            DESCollectionElementEditor.DESElementRemovedEvent += DESCollectionElementEditor_DESElementRemoved;
            DESCollectionElementEditor.DESElementPropertyValueChangedEvent += DESCollectionElementEditor_DESElementPropertyValueChangedEvent;
            cbbObject.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        #endregion

        #region Initialization Chart
        private void InitializeChart()
        {
            // Event Chart
            InitializeEventChart();

            // Queue Chart
            InitializeQueueChart();

            // Server Pie Chart
            InitializeServerPieChart();

            // Server Gantt Chart
            InitializeServerGanttChart();
        }

        private void InitializeEventChart()
        {
            Charting.ChartArea eventChartArea = chartEvent.ChartAreas[0];
            eventChartArea.AxisX.Title = "Time";
            eventChartArea.AxisX.MajorGrid.Enabled = false;
            eventChartArea.AxisX.LabelAutoFitMaxFontSize = 12;
            eventChartArea.AxisX.RoundAxisValues();

            eventChartArea.AxisY.Title = "Event";
            eventChartArea.AxisY.Interval = 1;
            eventChartArea.AxisY.Minimum = 0;
            eventChartArea.AxisY.Maximum = 5;
            eventChartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            eventChartArea.AxisY.LabelAutoFitMaxFontSize = 12;

            string[] events = { "Arrival", "ServiceDone", "BreakDown", "Repaired" };
            for (int i = 0; i < events.Length; i++)
            {
                Charting.CustomLabel label = new Charting.CustomLabel
                {
                    Text = events[i],
                    FromPosition = i + 0.6,
                    ToPosition = i + 1.4
                };
                chartEvent.ChartAreas[0].AxisY.CustomLabels.Add(label);
            }

            Charting.Series eventSeries = new Charting.Series
            {
                MarkerStyle = Charting.MarkerStyle.Cross,
                ChartType = Charting.SeriesChartType.Point,
                MarkerSize = 15,
                ChartArea = eventChartArea.Name,
                Legend = null,
                IsVisibleInLegend = false
            };
            chartEvent.Series.Add(eventSeries);
        }

        private void InitializeQueueChart()
        {
            Charting.ChartArea queueChartArea = chartQueue.ChartAreas[0];
            queueChartArea.AxisX.Minimum = 0;
            queueChartArea.AxisX.RoundAxisValues();
            queueChartArea.AxisX.Title = "Time";
            queueChartArea.AxisY.Title = "Size";
            chartQueue.Legends[0].Position.Auto = true;
            chartQueue.Legends[0].Docking = Charting.Docking.Bottom;
        }

        private void InitializeServerPieChart()
        {
            chartServerPie.Legends[0].Position.Auto = false;
            chartServerPie.Legends[0].Docking = Charting.Docking.Bottom;
        }

        private void InitializeServerGanttChart()
        {
            Charting.ChartArea serverGanttChartArea = chartServerGantt.ChartAreas[0];
            serverGanttChartArea.AxisX.MajorGrid.Enabled = false;
            serverGanttChartArea.AxisX.Minimum = 0;
            serverGanttChartArea.AxisX.LabelAutoFitMaxFontSize = 12;
        }
        #endregion

        #region File Operation
        private void btnNew_Click(object sender, EventArgs e)
        {
            panelMain.BackColor = Color.White;
            allElements.Clear();
            int count = cbbObject.Items.Count;
            for (int i = 1; i < count; i++)
                cbbObject.Items.RemoveAt(1);

            theModel = new DiscreteEventSimulationModel();
            cbbObject.SelectedIndex = 0;
            ppgObject.SelectedObject = theModel;

            panelMain.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (theModel == null) return;
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            StreamWriter sw = new StreamWriter(dlg.FileName);
            theModel.SaveToFile(sw);
            sw.Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            StreamReader sr = new StreamReader(dlg.FileName);

            panelMain.BackColor = Color.White;
            allElements.Clear();
            int count = cbbObject.Items.Count;
            for (int i = 1; i < count; i++)
                cbbObject.Items.RemoveAt(1);

            theModel = new DiscreteEventSimulationModel();
            cbbObject.SelectedIndex = 0;
            ppgObject.SelectedObject = theModel;
            theModel.ReadFromFile(sr);
            theModel.ExportDESElements(allElements);

            cbbObject.Items.Clear();
            cbbObject.Items.Add(theModel.Name);
            foreach (DESElement ele in allElements)
                cbbObject.Items.Add(ele.Name);

            cbbObject.SelectedIndex = 0;
            panelMain.Refresh();
            sr.Close();

            BtnSelect_Click(null, null);
        }
        #endregion

        #region Simulation Operation
        private void nudInterval_UpButtonClicked(object sender, MouseEventArgs e)
        {
            int value = Convert.ToInt32(nudInterval.TextBoxText) + 100;
            if (value > 5000)
                nudInterval.TextBoxText = "5000";
            else
                nudInterval.TextBoxText = $"{value}";
        }

        private void nudInterval_DownButtonClicked(object sender, MouseEventArgs e)
        {
            int value = Convert.ToInt32(nudInterval.TextBoxText) - 100;
            if (value < 10)
                nudInterval.TextBoxText = "10";
            else
                nudInterval.TextBoxText = $"{value}";
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            theModel.RunOneEvent();
            UpdateEventChart();
            ppgObject.Refresh();
            UpdateChartOfClient();
        }

        private void btnNextToEnd_Click(object sender, EventArgs e)
        {
            int sleepDuration = Convert.ToInt32(nudInterval.TextBoxText);

            while (true)
            {
                if (!theModel.RunOneEvent())
                    break;

                if (cbShowAnimation.Checked)
                {
                    ppgObject.Refresh();
                    UpdateChartOfClient();
                    UpdateEventChart();
                    Thread.Sleep(sleepDuration);
                }
            }

            tcMain.SelectedIndex = 1;
            btnNext.Enabled = false;
            btnNextToEnd.Enabled = false;

            tbSimulation.Text = theModel.GetSimulationResult();
            ResetChart();
            UpdateServerGanttChart();
            UpdateQueueChart();
        }

        private void UpdateServerGanttChart()
        {
            List<Server> servers = theModel.GetAllServers();
            int serverCount = servers.Count;
            int rowCount = (int)Math.Ceiling(serverCount / 3.0);
            int rowHeight = (int)(100.0 / rowCount) - 5;
            int pieWidth = 33;

            chartServerGantt.ChartAreas[0].AxisX.Maximum = serverCount + 1;

            int pieY = 0;
            int pieX = 0;
            int i = 0;
            foreach (Server server in servers)
            {
                // Update PieChart 
                CreatePieChartArea(server.PieStates);

                if (i % 3 == 0 && i != 0)
                {
                    pieY += rowHeight;
                    pieX = 0;
                }

                chartServerPie.ChartAreas[server.Name].Position.X = pieX;
                chartServerPie.ChartAreas[server.Name].Position.Y = pieY + 3;
                chartServerPie.ChartAreas[server.Name].Position.Height = rowHeight;
                chartServerPie.ChartAreas[server.Name].Position.Width = pieWidth;

                pieX += pieWidth;

                // Update GanChart
                server.GanttStates.ChartArea = chartServerGantt.ChartAreas[0].Name;
                chartServerGantt.Series.Add(server.GanttStates);
                server.GanttStates.IsVisibleInLegend = false;

                Charting.CustomLabel label = new Charting.CustomLabel
                {
                    Text = server.Name,
                    FromPosition = i + 0.6,
                    ToPosition = i + 1.4
                };
                chartServerGantt.ChartAreas[0].AxisX.CustomLabels.Add(label);
                i++;
            }
        }

        private void UpdateQueueChart()
        {
            List<TimeQueue> queues = theModel.GetAllQueues();
            double maxTime = 0;
            List<Color> colors = new List<Color> { Color.Blue, Color.Red, Color.Green, Color.Purple, Color.Brown };

            foreach (TimeQueue queue in queues)
            {
                if (queue.SeriesClients.Points.Max(p => p.XValue) > maxTime)
                    maxTime = queue.SeriesClients.Points.Max(p => p.XValue);

                queue.SeriesClients.ChartArea = chartQueue.ChartAreas[0].Name;
                queue.SeriesClients.Color = colors[queues.IndexOf(queue)];
                chartQueue.Series.Add(queue.SeriesClients);
            }

            chartQueue.ChartAreas[0].AxisX.RoundAxisValues();
            chartQueue.ChartAreas[0].AxisX.Maximum = Math.Ceiling(maxTime);
        }
        #endregion

        #region Operation trigger

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedElement == null)
            {
                MessageBox.Show("You must select a element to delete", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BtnSelect_Click(null, null);
                return;
            }
            if (selectedElement.GetType().Name == "ClientGenerator")
            {
                theModel.ClientGenerators.Remove((ClientGenerator)selectedElement);
            }

            RemoveElement(selectedElement);
   
            cbbObject.SelectedIndex = 0;
            selectedElement = null;
            ppgObject.SelectedObject = theModel;
            panelMain.Refresh();
        }

        private void btnModule_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Module);
        }

        private void btnServerTypes_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            this.Cursor = CursorManager.SetCursor((e.Item.Text == "Server") ? CursorType.Server : CursorType.Machine);

        }

        private void BtnQueue_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Queue);
        }

        private void BtnItinerary_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Itinerary);
        }

        private void BtnLink_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Link);
            selectedElement = null;
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Select);
        }
        private void BtnReleaser_Click(object sender, EventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Release);
        }
        private void Btn_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            this.Cursor = CursorManager.SetCursor(CursorType.Distribution);

            switch (e.Item.Text)
            {
                case "Uniform":
                    currentDistribution = ContinuousRandomGeneratorType.Uniform;
                    break;
                case "Exponential":
                    currentDistribution = ContinuousRandomGeneratorType.Exponential;
                    break;
                case "Gamma":
                    currentDistribution = ContinuousRandomGeneratorType.Gamma;
                    break;
                case "Chisquare":
                    currentDistribution = ContinuousRandomGeneratorType.Chisquare;
                    break;
                default:
                    currentDistribution = ContinuousRandomGeneratorType.None;
                    break;
            }
            selectedElement = null;
        }


        private void BtnReset_Click(object sender, EventArgs e)
        {
            //check generator
            if (theModel.ClientGenerators.Count == 0)
            {
                MessageBox.Show("At least one client generator must be generated", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (theModel.ClientGenerators[0].InterarrivalType == ContinuousRandomGeneratorType.None)
            {
                MessageBox.Show("No random variate generator is specified in time generator of the client generator ", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            theModel.ResetSimulation();
            UpdateEventChart();
            btnNext.Enabled = true;
            btnNextToEnd.Enabled = true;
            tbSimulation.Text = "";
        }
        #endregion

        #region Property Grid
        private void DESCollectionElementEditor_DESElementPropertyValueChangedEvent(object s, PropertyValueChangedEventArgs e)
        {
            panelMain.Refresh();
        }

        private void DESCollectionElementEditor_DESElementRemoved(object sender, DESElement element)
        {
            RemoveElement(element);
            panelMain.Refresh();
        }
        private void ppgObject_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            panelMain.Refresh();
            for (int i = 0; i < allElements.Count; i++)
            {
                cbbObject.Items[i + 1] = allElements[i].Name;
            }
        }

        private void cbbObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbObject.SelectedIndex == 0)
            {
                selectedElement = null;
                ppgObject.SelectedObject = theModel;
            }
            else
            {
                selectedElement = allElements[cbbObject.SelectedIndex - 1];
                ppgObject.SelectedObject = selectedElement;
                BtnSelect_Click(null, null);
            }
            panelMain.Refresh();
        }
        #endregion

        #region Add Element
        private void AddElement(DESElement element)
        {
            // Add element to collection and to drop-down list for the Property Grid.
            allElements.Add(element);
            cbbObject.Items.Add(element.Name);
        }

        private void DESCollectionElementEditor_DESElementAdded(object sender, DESElement element)
        {
            AddElement(element);
            panelMain.Refresh();
        }

        private void AddServiceNode()
        {
            ServiceNode serviceNode = new ServiceNode
            {
                Bound = theRectangle
            };

            AddElement(serviceNode);
            theModel.ServiceNodes.Add(serviceNode);
        }

        private void AddServer(ServiceNode serviceNode)
        {
            Server server = new Server
            {
                Bound = theRectangle,
                ParentNode = serviceNode
            };
            serviceNode.Servers.Add(server);

            AddElement(server);
        }

        private void AddQueue(ServiceNode serviceNode)
        {
            TimeQueue queue = new TimeQueue
            {
                Bound = theRectangle
            };
            serviceNode.Queues.Add(queue);
            AddElement(queue);
        }

        private void AddMachine(ServiceNode serviceNode)
        {
            Machine machine = new Machine
            {
                Bound = theRectangle,
                ParentNode = serviceNode
            };
            serviceNode.Servers.Add(machine);
            AddElement(machine);
        }

        private void AddItinerary()
        {
            Itinerary it = new Itinerary
            {
                Bound = theRectangle
            };
            AddElement(it);
            theModel.Itineraries.Add(it);
        }

        private void AddClientGenerator()
        {
            ClientGenerator cg = new ClientGenerator
            {
                Bound = theRectangle
            };
            AddElement(cg);
            theModel.ClientGenerators.Add(cg);
        }
        #endregion

        #region Remove Element
        private void RemoveElement(DESElement element)
        {
            // Remove element from collection and from drop-down list for the Property Grid.
            allElements.Remove(element);
            cbbObject.Items.Remove(element.Name);
        }
        #endregion

        #region Mouse Down
        private void PanelMain_MouseDown(object sender, MouseEventArgs e)
        {
            theRectangle = new Rectangle();
            startPoint = endPoint = panelMain.PointToScreen(e.Location);

            if (CursorManager.CurrentCursorType == CursorType.Select)
            {
                HandleSelectMode(e);
            }
            else if (CursorManager.CurrentCursorType == CursorType.Link)
            {
                HandleLinkMode(e);
            }
            else if (CursorManager.CurrentCursorType == CursorType.Distribution)
            {
                HandleDistributionMode(e);
            }
        }

        private int GetIndexofLatestElementAtTheLocation(Point location)
        {
            // Retrieve the element generated at the latest time on that position
            for (int i = allElements.Count - 1; i >= 0; i--)
            {
                if (allElements[i].Bound.Contains(location))
                {
                    return i;
                }
            }
            return -1;
        }

        private void HandleSelectMode(MouseEventArgs e)
        {
            int selectedIndex = GetIndexofLatestElementAtTheLocation(e.Location);
            if (selectedIndex == -1)
                return;


            selectedElement = allElements[selectedIndex];
            ppgObject.SelectedObject = selectedElement;
            cbbObject.SelectedIndex = selectedIndex + 1;
            Point pt = panelMain.PointToScreen(new Point(selectedElement.Bound.X, selectedElement.Bound.Y));
            theRectangle = new Rectangle(pt.X, pt.Y, selectedElement.Bound.Width, selectedElement.Bound.Height);
            isDrag = true;
        }

        private void HandleLinkMode(MouseEventArgs e)
        {
            for (int i = allElements.Count - 1; i >= 0; i--)
            {
                if (allElements[i].Bound.Contains(e.Location))
                {
                    selectedElement = allElements[i];
                    break;
                }
            }
        }

        private void HandleDistributionMode(MouseEventArgs e)
        {
            foreach (Itinerary it in theModel.Itineraries)
            {
                foreach (Visit visit in it.Visits)
                {
                    if (visit.Bound.Contains(e.Location))
                    {
                        visit.ServiceTimeGeneratorType = currentDistribution;
                        panelMain.Refresh();
                        return;
                    }
                }
            }

            foreach (ClientGenerator clientGenerator in theModel.ClientGenerators)
            {
                if (clientGenerator.Bound.Contains(e.Location))
                {
                    clientGenerator.InterarrivalType = currentDistribution;
                    panelMain.Refresh();
                    return;
                }
            }
        }

        #endregion

        #region Mouse Move
        private void panelMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (CursorManager.CurrentCursorType == CursorType.Link)
            {
                ControlPaint.DrawReversibleLine(startPoint, endPoint, Color.Red);
                endPoint = panelMain.PointToScreen(e.Location);
                ControlPaint.DrawReversibleLine(startPoint, endPoint, Color.Red);
            }
            else if (CursorManager.CurrentCursorType == CursorType.Select && isDrag)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);

                endPoint = panelMain.PointToScreen(e.Location);
                theRectangle.X += endPoint.X - startPoint.X;
                theRectangle.Y += endPoint.Y - startPoint.Y;

                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);

                startPoint = endPoint;
            }
            else
            {
                endPoint = panelMain.PointToScreen(e.Location);
                DrawBorderWhenMove();

            }
        }

        private void DrawBorderWhenMove()
        {
            // Clear the previous rectangle border
            if (theRectangle != Rectangle.Empty)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
            }

            // Calculate the endpoint and dimensions for the new rectangle, again using the PointToScreen method.
            theRectangle = new Rectangle(startPoint,
                new Size(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y));

            // Draw the new rectangle border by calling DrawReversibleFrame again.  
            ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
        }
        #endregion

        #region Mouse UP
        private ServiceNode GetServiceNodeContainsTheRectangle()
        {
            ServiceNode serviceNode;
            for (int i = 0; i < allElements.Count; i++)
            {
                if (!(allElements[i] is ServiceNode)) continue;

                serviceNode = (ServiceNode)allElements[i];
                if (serviceNode.Bound.Contains(theRectangle.Location))
                {

                    return serviceNode;
                }
            }

            return null;
        }
        private void panelMain_MouseUp(object sender, MouseEventArgs e)
        {

            if (CursorManager.CurrentCursorType == CursorType.Link)
            {
                int selectedIndex = GetIndexofLatestElementAtTheLocation(e.Location);
                if (selectedIndex == -1)
                {
                    selectedElement = null;
                    ControlPaint.DrawReversibleLine(startPoint, endPoint, Color.Red);
                    panelMain.Refresh();
                    return;
                }

                DESElement targetElement = allElements[selectedIndex];

                // From queue link to server
                if (selectedElement is TimeQueue queue)
                {
                    if (targetElement is Server server && !server.Queues.Contains(queue))
                    {
                        server.Queues.Add(queue);
                    }

                    else if(targetElement is Machine machine && !machine.Queues.Contains(queue))
                    {
                        machine.Queues.Add(queue);
                    }
                }

                // From client generator link to itinerary
                else if (selectedElement is ClientGenerator clientGenerator && targetElement is Itinerary itinerary && !clientGenerator.Itineraries.Contains(itinerary))
                {
                    clientGenerator.AddItinerary(itinerary);
                }

                // From itinerary link to servicenode
                // Ensure no matching service node in itinerary2's Visits collection
                else if (selectedElement is Itinerary itinerary2 && targetElement is ServiceNode serviceNode && !itinerary2.Visits.Any(visit => visit.TheNode == serviceNode))
                {
                  
                    Visit theVisit = new Visit
                    {
                        TheNode = serviceNode
                    };
                    cbbObject.Items.Add(theVisit.Name);
                    allElements.Add(theVisit);
                    itinerary2.Visits.Add(theVisit);
                }

                selectedElement = null;
                ControlPaint.DrawReversibleLine(startPoint, endPoint, Color.Red);
                panelMain.Refresh();

            }
            else if (CursorManager.CurrentCursorType == CursorType.Module)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);

                if (theRectangle.Height < 30 || theRectangle.Width < 30)
                {
                    theRectangle = new Rectangle();
                    return;
                }

                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                AddServiceNode();

            }
            else if (CursorManager.CurrentCursorType == CursorType.Queue)
            {

                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
                if (theRectangle.Height < 15 || theRectangle.Width < 15)
                {
                    theRectangle = new Rectangle();
                    return;
                }

                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                ServiceNode serviceNode = GetServiceNodeContainsTheRectangle();
                if (serviceNode == null)
                {
                    MessageBox.Show("You must put the queue in a service node", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddQueue(serviceNode);

            }
            else if (CursorManager.CurrentCursorType == CursorType.Server)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
                if (theRectangle.Height < 15 || theRectangle.Width < 15)
                {
                    theRectangle = new Rectangle();
                    return;
                }
                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                if (theRectangle.Width < theRectangle.Height)
                    theRectangle.Height = theRectangle.Width;
                else
                    theRectangle.Width = theRectangle.Height;

                ServiceNode serviceNode = GetServiceNodeContainsTheRectangle();
                if (serviceNode == null)
                {
                    MessageBox.Show("You must put the server in a service node", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddServer(serviceNode);

            }
            else if (CursorManager.CurrentCursorType == CursorType.Machine)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
                if (theRectangle.Height < 15 || theRectangle.Width < 15)
                {
                    theRectangle = new Rectangle();
                    return;
                }

                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                if (theRectangle.Width < theRectangle.Height)
                    theRectangle.Height = theRectangle.Width;
                else
                    theRectangle.Width = theRectangle.Height;


                ServiceNode serviceNode = GetServiceNodeContainsTheRectangle();
                if (serviceNode == null)
                {
                    MessageBox.Show("You must put the machine in a service node", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AddMachine(serviceNode);

            }
            else if (CursorManager.CurrentCursorType == CursorType.Select && isDrag)
            {
                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                selectedElement.Bound = theRectangle;
                isDrag = false;
            }
            else if (CursorManager.CurrentCursorType == CursorType.Itinerary)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
                if (theRectangle.Height < 15 || theRectangle.Width < 15)
                {
                    theRectangle = new Rectangle();
                    return;
                }

                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                if (theRectangle.Width < theRectangle.Height)
                    theRectangle.Height = theRectangle.Width;
                else
                    theRectangle.Width = theRectangle.Height;

                AddItinerary();
            }
            else if (CursorManager.CurrentCursorType == CursorType.Release)
            {
                ControlPaint.DrawReversibleFrame(theRectangle, Color.Red, FrameStyle.Dashed);
                if (theRectangle.Height < 15 || theRectangle.Height < 15)
                {
                    theRectangle = new Rectangle();
                    return;
                }

                if (theRectangle.Width < theRectangle.Height)
                    theRectangle.Height = theRectangle.Width;
                else
                    theRectangle.Width = theRectangle.Height;

                theRectangle.Location = panelMain.PointToClient(theRectangle.Location);
                AddClientGenerator();
            }
            
            panelMain.Refresh();
            theRectangle = new Rectangle();
        }
        #endregion

        #region Paint
        private void panelMain_Paint(object sender, PaintEventArgs e)
        {
            if (allElements.Count == 0) return;

            // Draw service nodes and line between parents and children.
            foreach (DESElement ele in allElements)
            {
                if (ele is ServiceNode)
                    ele.Draw(e.Graphics);

                else if (ele is Server || ele is Machine || ele is ClientGenerator)
                    ele.DrawLineToCollections(e.Graphics);
            }

            //itinerary 
            //Pentagon position with each service node.
            Dictionary<ServiceNode, int> itineraryTop = new Dictionary<ServiceNode, int>();
            Dictionary<ServiceNode, int> itineraryBottom = new Dictionary<ServiceNode, int>();
            Dictionary<ServiceNode, bool> itineraryOrderflag = new Dictionary<ServiceNode, bool>();

            foreach (ServiceNode node in theModel.ServiceNodes)
            {
                itineraryTop.Add(node, node.Bound.Y + node.Bound.Height / 2);
                itineraryBottom.Add(node, node.Bound.Y + node.Bound.Height / 2+10);
                itineraryOrderflag.Add(node, true);
            }

            ServiceNode serviceNode;
            Rectangle Bound;
            Rectangle circleBound;
            List<Color> colors = new List<Color>();
            List<Rectangle> circles = new List<Rectangle>();
            
            foreach (Itinerary it in theModel.Itineraries)
            {
                for (int i = 0; i < it.Visits.Count; i++)
                {
                    serviceNode = it.Visits[i].TheNode;
                    if (itineraryOrderflag[serviceNode])
                    {
                        int y = itineraryBottom[serviceNode];
                        Bound = new Rectangle(serviceNode.Bound.Right, y, it.Bound.Width / 2, it.Bound.Height / 2);
                        itineraryBottom[serviceNode] = Bound.Bottom+10;
                        itineraryOrderflag[serviceNode] = false;
                    }
                    else
                    {
                        int y = itineraryTop[serviceNode]- it.Bound.Height/2;
                        Bound = new Rectangle(serviceNode.Bound.Right, y, it.Bound.Width / 2, it.Bound.Height / 2);
                        itineraryTop[serviceNode] = Bound.Top+10;
                        itineraryOrderflag[serviceNode] = true;
                    }
                    
                    SolidBrush sb = new SolidBrush(it.BackColor);
                    PointF point1 = new PointF(Bound.X + Bound.Width / 4, Bound.Y);
                    PointF point2 = new PointF(Bound.Right, Bound.Y);
                    PointF point3 = new PointF(Bound.X, Bound.Y + Bound.Height / 2);
                    PointF point4 = new PointF(Bound.X + Bound.Width / 4, Bound.Bottom);
                    PointF point5 = new PointF(Bound.Right, Bound.Bottom);
                    PointF[] curvePoints = { point1, point2, point5, point4, point3 };
                    it.Visits[i].polygonPoints = curvePoints;
                    Point center = new Point(Bound.Left + Bound.Width / 2,
                                    Bound.Top + Bound.Height / 2);

                    colors.Add(it.BackColor);
                    int min;
                    min = Bound.Height * 3 / 4;
                    circleBound = new Rectangle(Bound.X + Bound.Width / 4, Bound.Y + Bound.Height / 4, min, min);
                    circles.Add(circleBound);
                    it.Visits[i].Bound = circleBound;

                    if (i == 0)
                        e.Graphics.DrawLine(Pens.Black, it.GetCenterPoint(), center);
                    else
                    {
                        Point p = new Point(it.Visits[i - 1].TheNode.Bound.X, it.Visits[i - 1].TheNode.Bound.Y + it.Visits[i - 1].TheNode.Bound.Height / 2);
                        e.Graphics.DrawLine(Pens.Black, p, center);
                    }
                }
            }

            foreach (DESElement ele in allElements)
            {
                if (ele is ServiceNode)
                    continue;
                ele.Draw(e.Graphics);
            }

            // Draw selection border
            if (selectedElement != null)
            {
                selectedElement.DrawSelectionBorder(e.Graphics);
            }
        }
        #endregion
     
        #region Chart Function

        public void CreatePieChartArea(Charting.Series series)
        {
            Charting.ChartArea chartArea = new Charting.ChartArea(series.Name);
            series.ChartArea = chartArea.Name;
            chartServerPie.Series.Add(series);
            chartServerPie.ChartAreas.Add(chartArea);

            Charting.Title tt = new Charting.Title();
            tt.Name = series.Name;
            chartServerPie.Titles.Add(tt);
            chartServerPie.Titles[tt.Name].Text = tt.Name;
            chartServerPie.Titles[tt.Name].DockedToChartArea = tt.Name;
            chartServerPie.Titles[tt.Name].IsDockedInsideChartArea = false;
        }
        public void ResetChart()
        {
            chartServerPie.Titles.Clear();
            chartQueue.Series.Clear();
            chartServerPie.Series.Clear();
            chartServerPie.ChartAreas.Clear();
            chartServerGantt.Series.Clear();
        }
        private void UpdateChartOfClient()
        {
            foreach (ServiceNode sn in theModel.ServiceNodes)
            {
                Graphics graphics = panelMain.CreateGraphics();
                foreach (TimeQueue queue in sn.Queues)
                    queue.Draw(graphics);

                foreach (Server server in sn.Servers)
                    server.Draw(graphics);
            }
        }

        private Color GetPointColor(DiscreteEventType eventType)
        {
            switch (eventType)
            {
                case DiscreteEventType.ClientArrival:
                case DiscreteEventType.ServerComplete:
                case DiscreteEventType.ServerRepair:
                    return Color.Blue;
                case DiscreteEventType.ServerBreakDown:
                    return Color.Red;
                default:
                    return Color.Black; // Handle the default color
            }
        }

        private int GetPointValue(DiscreteEventType eventType)
        {
            switch (eventType)
            {
                case DiscreteEventType.ClientArrival:
                    return 1;
                case DiscreteEventType.ServerComplete:
                    return 2;
                case DiscreteEventType.ServerBreakDown:
                    return 3;
                case DiscreteEventType.ServerRepair:
                    return 4;
                default:
                    return 0; // Handle the default value
            }
        }
        private void UpdateEventChart()
        {
            chartEvent.Series[0].Points.Clear();

            foreach (DiscreteEvent discreteEvent in theModel.FeatureEventList)
            {
                Charting.DataPoint point;
                Color pointColor = GetPointColor(discreteEvent.EventType);
                point = new Charting.DataPoint(discreteEvent.EventTime, GetPointValue(discreteEvent.EventType));
                point.Color = pointColor;
                chartEvent.Series[0].Points.Add(point);
            }

            chartEvent.Refresh();
        }

        private void chartQueue_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
