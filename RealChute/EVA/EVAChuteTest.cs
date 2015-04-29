using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.EVA
{
    public class EVAChuteTest : PartModule
    {
        [KSPField]
        public string transformName = string.Empty;
        [KSPField]
        public string animationName = string.Empty;

        private bool deployPressed
        {
            get
            {
                return (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.D))
                    || (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift));
            }
        }

        private bool hidePressed
        {
            get
            {
                return (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.H))
                    || (Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftShift));
            }
        }

        private Transform parachute = null;
        private Transform pilot = null;

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            this.parachute = this.part.FindModelTransform(this.transformName);
            this.parachute.gameObject.SetActive(false);
            this.part.InitiateAnimation(this.animationName);
        }

        private void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready || this.vessel == null || this.vessel.packed || !this.vessel.loaded) { return; }

            if (this.deployPressed)
            {
                if (!this.parachute.gameObject.activeSelf)
                {
                    this.parachute.gameObject.SetActive(true);
                }
                this.part.PlayAnimation(this.animationName, 0.25f);
            }

            if (this.hidePressed && this.parachute.gameObject.activeSelf)
            {
                this.parachute.gameObject.SetActive(false);
            }
        }
    }
}
