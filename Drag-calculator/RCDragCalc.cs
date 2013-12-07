using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealChute_drag_calculator
{
    public partial class RCDragCalc : Form
    {
        //Initiation
        public RCDragCalc()
        {
            InitializeComponent();
        }

        #region Variables
        //Variables
        public decimal gravity = kerbinGravity;
        public decimal density = kerbinDensity;
        public decimal pressure = kerbinPressure;
        public decimal scale = kerbinScale;
        public decimal mass = 10000;
        public decimal Cd = 1;
        public decimal speed = 10;
        public decimal deceleration = 10;
        public decimal parachutes = 1;
        public decimal speed2;
        public decimal diameter;
        #endregion

        #region Constants
        //Constants
        public const decimal kerbinGravity = 9.81m;
        public const decimal kerbinDensity = 1.223m;
        public const decimal kerbinPressure = 1;
        public const decimal kerbinScale = 5000;
        public const decimal dunaGravity = 2.943m;
        public const decimal dunaDensity = 0.245m;
        public const decimal dunaPressure = 0.2m;
        public const decimal dunaScale = 3000;
        public const decimal eveGravity = 16.677m;
        public const decimal eveDensity = 6.115m;
        public const decimal evePressure = 5;
        public const decimal eveScale = 7000;
        public const decimal laytheGravity = 7.848m;
        public const decimal laytheDensity = 0.978m;
        public const decimal laythePressure = 0.8m;
        public const decimal laytheScale = 4000;
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
            density = kerbinDensity;
            mass = 10000;
            Cd = 1;
            parachutes = 1;

            //Main tab values
            if (tabSelection.SelectedTab == tabSelection.TabPages["tabMains"])
            {
                rdoMains.Checked = true;
                gravity = kerbinGravity;
                pressure = kerbinPressure;
                scale = kerbinScale;
                speed = 10;
                numMass.Value = 10;
                numCd.Value = 1;
                lblSpeed.Text = "Wanted touchdown speed";
                numSpeed.Value = 10;
                numDeployment.Value = 700;
                numDeployment.Enabled = false;
                numParachutes.Value = 1;
                rdoKerbin.Checked = true;
                txtDiameter.Text = "0";
            }

            //Second tab values
            else if (tabSelection.SelectedTab == tabSelection.TabPages["tabDrags"])
            {
                speed = 100;
                deceleration = 10;
                numMassDrag.Value = 10;
                numCdDrag.Value = 1;
                numSpeedDrag.Value = 100;
                numDeceleration.Value = 10;
                numParachutesDrag.Value = 1;
                rdoKerbinDrag.Checked = true;
                txtDiameterDrag.Text = "0";
            }
        }
        #endregion

        #region Methods
        //Returns the density according to the altitude
        public decimal GetDensity(decimal altitude)
        {
            return kerbinDensity * (pressure * (decimal)Math.Exp((double)(-altitude / scale)));
        }
        #endregion

        #region Main tab
        //Kerbin checkbox
        private void rdoKerbin_CheckedChanged(object sender, EventArgs e)
        {
            gravity = kerbinGravity;
            density = kerbinDensity;
            pressure = kerbinPressure;
            scale = kerbinScale;
        }

        //Duna checkbox
        private void rdoDuna_CheckedChanged(object sender, EventArgs e)
        {
            gravity = dunaGravity;
            density = dunaDensity;
            pressure = dunaPressure;
            scale = dunaScale;
        }

        //Eve checkbox
        private void rdoEve_CheckedChanged(object sender, EventArgs e)
        {
            gravity = eveGravity;
            density = eveDensity;
            pressure = evePressure;
            scale = eveScale;
        }

        //Laythe checkbox
        private void rdoLaythe_CheckedChanged(object sender, EventArgs e)
        {
            gravity = laytheGravity;
            density = laytheDensity;
            pressure = laythePressure;
            scale = laytheScale;
        }

        //Main chutes checkbox
        private void rdoMains_CheckedChanged(object sender, EventArgs e)
        {
            numMass.Value = 10;
            mass = 10000;
            lblSpeed.Text = "Wanted touchdown speed";
            numSpeed.Value = 10;
            numSpeed.Maximum = 500;
            speed = 10;
            numDeployment.Enabled = false;

            if (rdoKerbin.Checked)
            {
                density = kerbinDensity;
                pressure = kerbinPressure;
                scale = kerbinScale;
            }

            else if (rdoDuna.Checked)
            {
                density = dunaDensity;
                pressure = dunaPressure;
                scale = dunaScale;
            }

            else if (rdoEve.Checked)
            {
                density = eveDensity;
                pressure = evePressure;
                scale = eveScale;
            }

            else if (rdoLaythe.Checked)
            {
                density = laytheDensity;
                pressure = laythePressure;
                scale = laytheScale;
            }
        }

        //Drag chutes checkbox
        private void rdoDrogues_CheckedChanged(object sender, EventArgs e)
        {
            numMass.Value = 50;
            mass = 50000;
            lblSpeed.Text = "Wanted speed at deployment";
            numSpeed.Value = 80;
            numSpeed.Maximum = 2000;
            speed = 80;
            numDeployment.Enabled = true;
            numDeployment.Value = 700;
            density = GetDensity(700);
        }

        //Mass selection
        private void numMass_ValueChanged(object sender, EventArgs e)
        {
            mass = numMass.Value * 1000;
        }

        //Cd selection
        private void numCd_ValueChanged(object sender, EventArgs e)
        {
            Cd = numCd.Value;
        }

        //Speed selection
        private void numSpeed_ValueChanged(object sender, EventArgs e)
        {
            speed = numSpeed.Value;
        }

        //Deployment altitude selection
        private void numDeployment_ValueChanged(object sender, EventArgs e)
        {
            density = GetDensity(numDeployment.Value);
        }

        //Parachute count selection
        private void numParachutes_ValueChanged(object sender, EventArgs e)
        {
            parachutes = numParachutes.Value;
        }

        //Calculations
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //If a parameter is null
            if (density <= 0 || gravity <= 0 || mass <= 0 || Cd <= 0 || speed <= 0 || parachutes <= 0)
            {
                return;
            }
            
            //Calculates diameter
            else
            {
                speed2 = (decimal)Math.Pow((double)speed, 2);
                diameter = (decimal)Math.Sqrt((double)((8 * mass * gravity) / ((decimal)Math.PI * speed2 * Cd * density * parachutes)));
                txtDiameter.Text = diameter.ToString("#0.00");
            }
        }
        #endregion

        #region Drag tab
        //Kerbin checkbox
        private void rdoKerbinDrag_CheckedChanged(object sender, EventArgs e)
        {
            density = kerbinDensity;
        }

        //Duna checkbox
        private void rdoDunaDrag_CheckedChanged(object sender, EventArgs e)
        {
            density = dunaDensity;
        }

        //Eve checkbox
        private void rdoEveDrag_CheckedChanged(object sender, EventArgs e)
        {
            density = eveDensity;
        }

        //Laythe checkbox
        private void rdoLaytheDrag_CheckedChanged(object sender, EventArgs e)
        {
            density = laytheDensity;
        }

        //Mass selection
        private void numMassDrag_ValueChanged(object sender, EventArgs e)
        {
            mass = numMassDrag.Value * 1000;
        }

        //Cd selection
        private void numCdDrag_ValueChanged(object sender, EventArgs e)
        {
            Cd = numCdDrag.Value;
        }

        //Landing speed selection
        private void numSpeedDrag_ValueChanged(object sender, EventArgs e)
        {
            speed = numSpeedDrag.Value;
        }

        //Deceleration selection
        private void numDeceleration_ValueChanged(object sender, EventArgs e)
        {
            deceleration = numDeceleration.Value;
        }

        //Parachute count selection
        private void numParachutesDrag_ValueChanged(object sender, EventArgs e)
        {
            parachutes = numParachutesDrag.Value;
        }

        //Calculations
        private void btnCalculateDrag_Click(object sender, EventArgs e)
        {
            //If a parameter is null
            if (density <= 0 || deceleration <= 0 || mass <= 0 || Cd <= 0 || speed <= 0 || parachutes <= 0)
            {
                return;
            }

            //Calculates the diameter
            else
            {
                speed2 = (decimal)Math.Pow((double)speed, 2);
                diameter = (decimal)Math.Sqrt((double)((8 * mass * deceleration) / ((decimal)Math.PI * speed2 * Cd * density * parachutes)));
                txtDiameterDrag.Text = diameter.ToString("#0.00");
            }
        }
        #endregion
    }
}
