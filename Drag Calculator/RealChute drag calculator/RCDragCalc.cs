using System;
using System.Windows.Forms;

/* The RealChute drag calculator was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute_drag_calculator
{
    public partial class RCDragCalc : Form
    {
        //Initiation
        public RCDragCalc()
        {
            InitializeComponent();
            cmbMaterial.SelectedIndex = 0;
            cmbMaterialDrag.SelectedIndex = 0;
        }

        //Body class
        private class Body
        {
            #region Propreties
            /// <summary>
            /// Surface gravity of the body (m/s²)
            /// </summary>
            public double gravity { get; private set; }

            /// <summary>
            /// Scale height of the body
            /// </summary>
            public double scale { get; private set; }

            /// <summary>
            /// Atmospheric pressure of the body ASL
            /// </summary>
            public double pressure { get; private set; }

            /// <summary>
            /// Atmospheric density of the boddy ASL
            /// </summary>
            public double density
            {
                get { return this.gravity * 1.223d; }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Assigns the readonly values according to the body
            /// </summary>
            /// <param name="name">Name of the body</param>
            public Body(string name)
            {
                switch (name)
                {
                    case "Kerbin":
                        {
                            gravity = 9.807d;
                            pressure = 1d;
                            scale = 5000d;
                            break;
                        }
                    case "Duna":
                        {
                            gravity = 2.943d;
                            pressure = 0.2d;
                            scale = 3000d;
                            break;
                        }

                    case "Eve":
                        {
                            gravity = 16.677d;
                            pressure = 5d;
                            scale = 7000;
                            break;
                        }
                    case "Laythe":
                        {
                            gravity = 7.848d;
                            pressure = 0.8d;
                            scale = 4000;
                            break;
                        }
                    default:
                        break;
                }
            }
            #endregion
        }

        #region Fields
        //Variables
        private double mass = 10000d;
        private double Cd = 1d;
        private double altitude = 0d;
        private double speed = 100d;
        private double deceleration = 10d;
        private double parachutes = 1d;
        private double diameter = 0d;
        private Body body = new Body("Kerbin");
        private readonly double[] materialsCd = { 1d, 1.25d, 0.75d };
        #endregion

        #region Methods
        //Returns the density according to the altitude
        private double GetDensityAtAlt(double altitude)
        {
            return 1.223d * body.pressure * Math.Exp(-altitude / body.scale);
        }
        #endregion

        #region Menus
        //Quit menu
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        //Tab change
        private void tabSelection_Selected(object sender, TabControlEventArgs e)
        {
            //Non changing values
            body = new Body("Kerbin");
            mass = 10000d;
            Cd = 1d;
            parachutes = 1d;
            altitude = 0d;
            speed = 100d;

            //Main tab values
            if (tabSelection.SelectedTab == tabSelection.TabPages["tabMains"])
            {
                rdoMains.Checked = true;
                numMass.Value = 10m;
                chkManualCd.Checked = false;
                cmbMaterial.SelectedIndex = 0;
                lblCd.Text = "Material";
                numCd.Value = 1m;
                lblSpeed.Text = "Wanted touchdown speed";
                numSpeed.Value = 10m;
                altitude = 0d;
                numDeployment.Value = 0m;
                numDeployment.Enabled = false;
                numParachutes.Value = 1m;
                rdoKerbin.Checked = true;
                txtDiameter.Text = "0";
            }

            //Second tab values
            else if (tabSelection.SelectedTab == tabSelection.TabPages["tabDrags"])
            {
                deceleration = 10d;
                numMassDrag.Value = 10m;
                chkManualCdDrag.Checked = false;
                cmbMaterialDrag.SelectedIndex = 0;
                lblCdDrag.Text = "Material";
                numCdDrag.Value = 1m;
                numSpeedDrag.Value = 10m;
                numDeceleration.Value = 10m;
                numParachutesDrag.Value = 1m;
                rdoKerbinDrag.Checked = true;
                txtDiameterDrag.Text = "0";
            }
        }
        #endregion

        #region Main tab
        //Kerbin checkbox
        private void rdoKerbin_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Kerbin");
        }

        //Duna checkbox
        private void rdoDuna_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Duna");
        }

        //Eve checkbox
        private void rdoEve_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Eve");
        }

        //Laythe checkbox
        private void rdoLaythe_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Laythe");
        }

        //Main chutes checkbox
        private void rdoMains_CheckedChanged(object sender, EventArgs e)
        {
            numMass.Value = 10m;
            mass = 10000d;
            lblSpeed.Text = "Wanted touchdown speed";
            numSpeed.Value = 10m;
            numSpeed.Maximum = 500m;
            speed = 100d;
            altitude = 0d;
            numDeployment.Enabled = false;
            numDeployment.Value = 0m;
        }

        //Drogue chutes checkbox
        private void rdoDrogues_CheckedChanged(object sender, EventArgs e)
        {
            numMass.Value = 50m;
            mass = 50000d;
            lblSpeed.Text = "Wanted speed at deployment";
            numSpeed.Value = 80m;
            numSpeed.Maximum = 2000m;
            speed = 6400d;
            altitude = 700d;
            numDeployment.Enabled = true;
            numDeployment.Value = 700m;
        }

        //Mass selection
        private void numMass_ValueChanged(object sender, EventArgs e)
        {
            mass = (double)numMass.Value * 1000d;
        }

        //Selects manual or automatic Cd selection
        private void chkManualCd_CheckedChanged(object sender, EventArgs e)
        {
            if (chkManualCd.Checked)
            {
                lblCd.Text = "Drag coefficient";
                cmbMaterial.Visible = false;
                numCd.Visible = true;
                Cd = (double)numCd.Value;
            }

            else
            {
                lblCd.Text = "Material";
                cmbMaterial.Visible = true;
                numCd.Visible = false;
                Cd = materialsCd[cmbMaterial.SelectedIndex];
            }
        }

        //Automatic Cd selection
        private void cmbMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cd = materialsCd[cmbMaterial.SelectedIndex];
        }

        //Manual Cd selection
        private void numCd_ValueChanged(object sender, EventArgs e)
        {
            Cd = (double)numCd.Value;
        }

        //Speed selection
        private void numSpeed_ValueChanged(object sender, EventArgs e)
        {
            speed = Math.Pow((double)numSpeed.Value, 2d);
        }

        //Deployment altitude selection
        private void numDeployment_ValueChanged(object sender, EventArgs e)
        {
            altitude = (double)numDeployment.Value;
        }

        //Parachute count selection
        private void numParachutes_ValueChanged(object sender, EventArgs e)
        {
            parachutes = (double)numParachutes.Value;
        }

        //Calculations
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //If a parameter is null
            if (body.density <= 0 || body.gravity <= 0 || mass <= 0 || Cd <= 0 || speed <= 0 || parachutes <= 0) { return; }
            
            //Calculates diameter
            diameter = Math.Sqrt((8d * mass * body.gravity) / (Math.PI * speed * Cd * GetDensityAtAlt(altitude) * parachutes));
            txtDiameter.Text = diameter.ToString("0.##");
        }
        #endregion

        #region Drag tab
        //Kerbin checkbox
        private void rdoKerbinDrag_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Kerbin");
        }

        //Duna checkbox
        private void rdoDunaDrag_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Duna");
        }

        //Eve checkbox
        private void rdoEveDrag_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Eve");
        }

        //Laythe checkbox
        private void rdoLaytheDrag_CheckedChanged(object sender, EventArgs e)
        {
            body = new Body("Laythe");
        }

        //Mass selection
        private void numMassDrag_ValueChanged(object sender, EventArgs e)
        {
            mass = (double)numMassDrag.Value * 1000d;
        }

        //Selects manual or automatic Cd selection
        private void chkManualCdDrag_CheckedChanged(object sender, EventArgs e)
        {
            if (chkManualCdDrag.Checked)
            {
                lblCdDrag.Text = "Drag coefficient";
                cmbMaterialDrag.Visible = false;
                numCdDrag.Visible = true;
                Cd = (double)numCdDrag.Value;
            }

            else
            {
                lblCdDrag.Text = "Material";
                cmbMaterialDrag.Visible = true;
                numCdDrag.Visible = false;
                Cd = materialsCd[cmbMaterialDrag.SelectedIndex];
            }
        }

        //Automatic Cd selection
        private void cmbMaterialDrag_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cd = materialsCd[cmbMaterialDrag.SelectedIndex];
        }

        //Manual Cd selection
        private void numCdDrag_ValueChanged(object sender, EventArgs e)
        {
            Cd = (double)numCdDrag.Value;
        }

        //Landing speed selection
        private void numSpeedDrag_ValueChanged(object sender, EventArgs e)
        {
            speed = Math.Pow((double)numSpeedDrag.Value, 2d);
        }

        //Deceleration selection
        private void numDeceleration_ValueChanged(object sender, EventArgs e)
        {
            deceleration = (double)numDeceleration.Value;
        }

        //Parachute count selection
        private void numParachutesDrag_ValueChanged(object sender, EventArgs e)
        {
            parachutes = (double)numParachutesDrag.Value;
        }

        //Calculations
        private void btnCalculateDrag_Click(object sender, EventArgs e)
        {
            //If a parameter is null
            if (body.density <= 0 || deceleration <= 0 || mass <= 0 || Cd <= 0 || speed <= 0 || parachutes <= 0) { return; }

            //Calculates the diameter
            diameter = Math.Sqrt(((8 * mass * deceleration) / (Math.PI * speed * Cd * body.density * parachutes)));
            txtDiameterDrag.Text = diameter.ToString("0.##");
        }
        #endregion
    }
}
