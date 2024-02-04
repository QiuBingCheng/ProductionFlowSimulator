using DiscreteEventSimulationLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
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
        bool isDrag;

        public MainForm()
        {
            InitializeComponent();
            InitializeChart();
            InitializeElementPropertyEditor();
            this.CenterToScreen();
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
            cbbObject.BackColor = Color.Blue;
            cbbObject.ForeColor = Color.AntiqueWhite;
        }

        private void InitializeChart()
        {
            chartEvent.ChartAreas[0].AxisX.Title = "Time";
            chartEvent.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartEvent.ChartAreas[0].AxisX.LabelAutoFitMaxFontSize = 12;
            chartEvent.ChartAreas[0].AxisX.RoundAxisValues();
            chartEvent.ChartAreas[0].AxisY.Title = "Event";
            chartEvent.ChartAreas[0].AxisY.Interval = 1;
            chartEvent.ChartAreas[0].AxisY.Minimum = 0;
            chartEvent.ChartAreas[0].AxisY.Maximum = 5;
            chartEvent.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartEvent.ChartAreas[0].AxisY.LabelAutoFitMaxFontSize = 12;

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

            Charting.Series series = new Charting.Series();
            series.MarkerStyle = Charting.MarkerStyle.Cross;
            series.ChartType = Charting.SeriesChartType.Point;
            series.MarkerSize = 15;
            series.ChartArea = chartEvent.ChartAreas[0].Name;
            series.Legend = null;
            series.IsVisibleInLegend = false;
            chartEvent.Series.Add(series);

            //
            chartQueue.ChartAreas[0].AxisX.Minimum = 0;
            chartQueue.ChartAreas[0].AxisX.RoundAxisValues();
            chartQueue.ChartAreas[0].AxisX.Title = "Time";
            chartQueue.ChartAreas[0].AxisY.Title = "Size";
            chartQueue.Legends[0].Position.Auto = true;
            chartQueue.Legends[0].Docking = Charting.Docking.Bottom;

            //
            chartServerPie.Legends[0].Position.Auto = false;
            chartServerPie.Legends[0].Docking = Charting.Docking.Bottom;

            //
            chartServerGantt.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartServerGantt.ChartAreas[0].AxisX.Minimum = 0;
            chartServerGantt.ChartAreas[0].AxisX.LabelAutoFitMaxFontSize = 12;
        }

        #region operation trigger
  
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
            allElements.Remove(selectedElement);
            cbbObject.Items.Remove(selectedElement.Name);
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
        ContinuousRandomGeneratorType currentDistribution = ContinuousRandomGeneratorType.None;
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
        }
        #endregion

        private void DESCollectionElementEditor_DESElementPropertyValueChangedEvent(object s, PropertyValueChangedEventArgs e)
        {

            panelMain.Refresh();
        }

        private void DESCollectionElementEditor_DESElementRemoved(object sender, DESElement e)
        {
            if (e.GetType().Name == "Itinerary")
            {

            }
            allElements.Remove(e);
            cbbObject.Items.Remove(e.Name);
            panelMain.Refresh();
        }

        private void DESCollectionElementEditor_DESElementAdded(object sender, DESElement e)
        {
            allElements.Add(e);
            cbbObject.Items.Add(e.Name);
            panelMain.Refresh();
        }

        #region MouseDown
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

        private void HandleSelectMode(MouseEventArgs e)
        {
            for (int i = allElements.Count - 1; i >= 0; i--)
            {
                if (allElements[i].Bound.Contains(e.Location))
                {
                    selectedElement = allElements[i];
                    ppgObject.SelectedObject = selectedElement;
                    cbbObject.SelectedIndex = i + 1;

                    Point pt = panelMain.PointToScreen(new Point(selectedElement.Bound.X, selectedElement.Bound.Y));
                    theRectangle = new Rectangle(pt.X, pt.Y, selectedElement.Bound.Width, selectedElement.Bound.Height);
                    isDrag = true;
                    break;
                }
            }
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
                        HandleVisitDistribution(visit);
                        return;
                    }
                }
            }

            foreach (ClientGenerator clientGenerator in theModel.ClientGenerators)
            {
                if (clientGenerator.Bound.Contains(e.Location))
                {
                    HandleClientGeneratorDistribution(clientGenerator);
                    return;
                }
            }
        }

        private void HandleVisitDistribution(Visit visit)
        {
            visit.ServiceTimeGeneratorType = currentDistribution;
            panelMain.Refresh();
        }

        private void HandleClientGeneratorDistribution(ClientGenerator clientGenerator)
        {
            clientGenerator.InterarrivalType = currentDistribution;
            panelMain.Refresh();
        }

        #endregion
       

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

        private void AddServiceNode()
        {
            ServiceNode serviceNode = new ServiceNode
            {
                Bound = theRectangle
            };
            allElements.Add(serviceNode);
            cbbObject.Items.Add(serviceNode.Name);
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
            allElements.Add(server);
            cbbObject.Items.Add(server.Name);
        }
      
        private void AddQueue(ServiceNode serviceNode)
        {
            TimeQueue queue = new TimeQueue
            {
                Bound = theRectangle
            };
            serviceNode.Queues.Add(queue);
            allElements.Add(queue);
            cbbObject.Items.Add(queue.Name);
        }

        private void AddMachine(ServiceNode serviceNode)
        {
            Machine machine = new Machine
            {
                Bound = theRectangle,
                ParentNode = serviceNode
            };
            serviceNode.Servers.Add(machine);
            allElements.Add(machine);
            cbbObject.Items.Add(machine.Name);
        }

        private void AddItinerary()
        {
            Itinerary it = new Itinerary
            {
                Bound = theRectangle
            };
            allElements.Add(it);
            cbbObject.Items.Add(it.Name);
            theModel.Itineraries.Add(it);
        }

        private void AddClientGenerator()
        {
            ClientGenerator cg = new ClientGenerator
            {
                Bound = theRectangle
            };
            allElements.Add(cg);
            cbbObject.Items.Add(cg.Name);
            theModel.ClientGenerators.Add(cg);
        }

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
            //ControlPaint.DrawReversibleLine(mouseDownScreenPoint, currentScreenPoint, Color.Red);

            if (CursorManager.CurrentCursorType == CursorType.Itinerary)
            {
                for (int i = allElements.Count - 1; i >= 0; i--)
                {
                    if (allElements[i].Bound.Contains(e.Location))
                    {
                        string fromElement = selectedElement.GetType().Name;
                        string toElement = allElements[i].GetType().Name;

                        if (fromElement == "TimeQueue" && toElement == "Server")
                        {

                            if (!((Server)allElements[i]).Queues.Contains((TimeQueue)selectedElement)) { 
                                ((Server)allElements[i]).Queues.Add((TimeQueue)selectedElement);
                            }
                        }
                        else if (fromElement == "TimeQueue" && toElement == "Machine")
                        {
                            if (!((Machine)allElements[i]).Queues.Contains((TimeQueue)selectedElement)){
                                ((Machine)allElements[i]).Queues.Add((TimeQueue)selectedElement);
                            }
                            
                        }
                        else if (fromElement == "ClientGenerator" && toElement == "Itinerary")
                        {
                            if (!((ClientGenerator)selectedElement).Itineraries.Contains((Itinerary)allElements[i]))
                            {
                                ((ClientGenerator)selectedElement).Itineraries.Add((Itinerary)allElements[i]);
                                ((ClientGenerator)selectedElement).GenerationWeights.Add(100);
                            }
                        }
                        else if (fromElement == "Itinerary" && toElement == "ServiceNode")
                        {
                            Itinerary it = (Itinerary)selectedElement;
                            foreach (Visit visit in it.Visits )
                            {
                                if(visit.TheNode == (ServiceNode)allElements[i])
                                {
                                    selectedElement = null;
                                    return;
                                }
                            }
                            Visit theVisit = new Visit();
                            theVisit.TheNode = (ServiceNode)allElements[i];
                            cbbObject.Items.Add(theVisit.Name);
                            allElements.Add(theVisit);
                            ((Itinerary)selectedElement).Visits.Add(theVisit);
                        }
                        selectedElement = null;
                        panelMain.Refresh();
                        return;
                    }
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
                if (serviceNode is null)
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
                if (serviceNode is null)
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
                if (serviceNode is null)
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

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {
            //service node
            if (allElements.Count == 0) return;

            //draw line
            foreach (DESElement ele in allElements)
            {
                if (ele.GetType().Name == "ServiceNode")
                    ele.Draw(e.Graphics);

                else if (ele.GetType().Name == "Server")
                {
                    Server server = (Server)ele;
                    foreach (TimeQueue queue in server.Queues)
                        e.Graphics.DrawLine(Pens.Black, queue.GetCenterPoint(), server.GetCenterPoint());
                       
                }
                else if(ele.GetType().Name == "Machine")
                {
                    Machine machine = (Machine)ele;
                    foreach (TimeQueue queue in machine.Queues)
                        e.Graphics.DrawLine(Pens.Black, queue.GetCenterPoint(), machine.GetCenterPoint());
                }
                else if (ele.GetType().Name == "ClientGenerator")
                {
                    ClientGenerator cg = (ClientGenerator)ele;
                    foreach (Itinerary it in cg.Itineraries)
                        e.Graphics.DrawLine(Pens.Black, cg.GetCenterPoint(), it.GetCenterPoint());
                }
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

            ServiceNode sn;
            Rectangle Bound;
            Rectangle circleBound;
            List<Color> colors = new List<Color>();
            List<Rectangle> circles = new List<Rectangle>();
            
            foreach (Itinerary it in theModel.Itineraries)
            {
                for (int i = 0; i < it.Visits.Count; i++)
                {
                    sn = it.Visits[i].TheNode;
                    if (itineraryOrderflag[sn])
                    {
                        int y = itineraryBottom[sn];
                        Bound = new Rectangle(sn.Bound.Right, y, it.Bound.Width / 2, it.Bound.Height / 2);
                        itineraryBottom[sn] = Bound.Bottom+10;
                        itineraryOrderflag[sn] = false;
                    }
                    else
                    {
                        int y = itineraryTop[sn]- it.Bound.Height/2;
                        Bound = new Rectangle(sn.Bound.Right, y, it.Bound.Width / 2, it.Bound.Height / 2);
                        itineraryTop[sn] = Bound.Top+10;
                        itineraryOrderflag[sn] = true;
                    }
                    
                    SolidBrush sb = new SolidBrush(it.BackColor);
                    PointF point1 = new PointF(Bound.X + Bound.Width / 4, Bound.Y);
                    PointF point2 = new PointF(Bound.Right, Bound.Y);
                    PointF point3 = new PointF(Bound.X, Bound.Y + Bound.Height / 2);
                    PointF point4 = new PointF(Bound.X + Bound.Width / 4, Bound.Bottom);
                    PointF point5 = new PointF(Bound.Right, Bound.Bottom);
                    PointF[] curvePoints = { point1, point2, point5, point4, point3 };
                    it.Visits[i].polygonPoints = curvePoints;
                    //points.Add(curvePoints);
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
                if (ele.GetType().Name != "ServiceNode")
                    ele.Draw(e.Graphics);
            }

            if (selectedElement != null)
            {
                selectedElement.DrawSelectionHandles(e.Graphics);
            }
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

        //file operation
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

        //
       

        private void UpdateEventChart()
        {
            chartEvent.Series[0].Points.Clear();
            foreach (DiscreteEvent discreteEvent in theModel.FeatureEventList)
            {
                // string[] events = { "Arrival", "ServiceDone", "BreakDown", "Repaired" };
                Charting.DataPoint point;
                switch (discreteEvent.GetType().Name)
                {
                    case "ClientArrivalEvent":
                        point = new Charting.DataPoint(discreteEvent.EventTime, 1);
                        point.Color = Color.Blue;
                        chartEvent.Series[0].Points.Add(point);
                        break;
                    case "ServiceCompleteEvent":
                        point = new Charting.DataPoint(discreteEvent.EventTime, 2);
                        point.Color = Color.Blue;
                        chartEvent.Series[0].Points.Add(point);
                        break;
                    case "BreakDownEvent":
                        point = new Charting.DataPoint(discreteEvent.EventTime, 3);
                        point.Color = Color.Red;
                        chartEvent.Series[0].Points.Add(point);
                        break;
                    case "RepairEvent":
                        point = new Charting.DataPoint(discreteEvent.EventTime, 4);
                        point.Color = Color.Blue;
                        chartEvent.Series[0].Points.Add(point);
                        break;
                }
            }
            chartEvent.Refresh();
        }

        private void panelMain_Scroll(object sender, ScrollEventArgs e)
        {
            panelMain.Refresh();
        }
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

        private void ppgObject_ControlAdded(object sender, ControlEventArgs e)
        {
            
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
                if (theModel.RunOneEvent())
                {
                    if (cbShowAnimation.Checked)
                    {
                        ppgObject.Refresh();
                        UpdateChartOfClient();
                        UpdateEventChart();
                        Thread.Sleep(sleepDuration);
                    }
                }
                else
                {
                    tcMain.SelectedIndex = 1;
                    btnNext.Enabled = false;
                    btnNextToEnd.Enabled = false;
                    break;
                }
            }
            tbSimulation.Text = theModel.GetSimulationResult();

            //chart
            ResetChart();

            //server
            List<Server> servers = theModel.GetAllServers();
            int serverCount = servers.Count();
            chartServerGantt.ChartAreas[0].AxisX.Maximum = serverCount+1;

            int pieY = 0;
            int pieX = 0;
            int rowCount = (int)Math.Ceiling(serverCount / 3.0);
            int rowHeight = (int)(100.0/ rowCount)-5;
            int pieWidth = 33;
            int i = 0;
            foreach (Server server in servers)
            {
                //pie chart
                CreatePieChartArea(server.PieStates);
                //next row
                if(i%3 == 0 && i!=0)
                {
                    pieY += rowHeight;
                    pieX = 0;
                }
                chartServerPie.ChartAreas[server.Name].Position.X = pieX;
                chartServerPie.ChartAreas[server.Name].Position.Y = pieY+3;
                chartServerPie.ChartAreas[server.Name].Position.Height = rowHeight;
                chartServerPie.ChartAreas[server.Name].Position.Width = pieWidth;
                pieX += pieWidth;
                
                //gantt chart
                server.GanttStates.ChartArea = chartServerGantt.ChartAreas[0].Name;
                chartServerGantt.Series.Add(server.GanttStates);
                server.GanttStates.IsVisibleInLegend = false;

                Charting.CustomLabel label = new Charting.CustomLabel(); 
                label.Text = server.Name;
                label.FromPosition = i + 0.6;
                label.ToPosition = i + 1.4;
                chartServerGantt.ChartAreas[0].AxisX.CustomLabels.Add(label);

                i++;
            }

            //queue
            List<TimeQueue> queues = theModel.GetAllQueues();
            double maxTime = 0;
            List<Color> colors = new List<Color> { Color.Blue,Color.Red,Color.Green,Color.Purple,Color.Brown};
            i = 0;
            foreach (TimeQueue queue in queues)
            {
                if (queue.SeriesClients.Points.Max(p=>p.XValue)> maxTime)
                    maxTime = queue.SeriesClients.Points.Max(p => p.XValue);

                queue.SeriesClients.ChartArea = chartQueue.ChartAreas[0].Name;
                queue.SeriesClients.Color = colors[i++];
                chartQueue.Series.Add(queue.SeriesClients);
            }
            chartQueue.ChartAreas[0].AxisX.RoundAxisValues();
            chartQueue.ChartAreas[0].AxisX.Maximum = maxTime;

        }
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

        private void chartQueue_Click(object sender, EventArgs e)
        {

        }

      
    }
}
