using System.Text;
using RealChute.Extensions;
using RealChute.Utils;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalDumper : MonoBehaviour
    {
        private bool printPressed
        {
            get
            {
                return (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.P))
                    || (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftShift));
            }
        }

        private bool hidePressed
        {
            get
            {
                return (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.O))
                    || (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftShift));
            }
        }

        private void Update()
        {
            if (this.printPressed)
            {
                PrintKerbalModel();
            }

            if (this.hidePressed)
            {
                DisableJetpack();
            }
        }

        private void PrintKerbalModel()
        {
            if (!FlightGlobals.ready || FlightGlobals.ActiveVessel == null || !FlightGlobals.ActiveVessel.isEVA) { return; }
            Part part = FlightGlobals.ActiveVessel.Parts[0];
            if (part == null || part.transform == null || part.gameObject == null) { return; }
            ProtoCrewMember kerbal = part.protoModuleCrew[0];
            StringBuilder b = new StringBuilder("[KerbalDumper]: Dumping Kerbal data\n\n");
            b.Append("Kerbal name: ").AppendLine(kerbal.name);
            b.Append("Kerbal profession: ").AppendLine(kerbal.experienceTrait.Title);
            b.Append("Profession level: ").AppendLine(kerbal.experienceLevel.ToString());
            b.AppendLine("\nTransform tree:");
            RCUtils.PrintChildren(part.transform, b);
            print(b.ToString());
        }

        private void DisableJetpack()
        {
            Transform parent = FlightGlobals.ActiveVessel.Parts[0].transform;
            Transform jetpack = parent.GetChild(5).GetChild(3);
            //Flag decals
            parent.GetChild(1).GetChild(0).gameObject.SetActive(false);
            //Jetpack base
            jetpack.GetChild(2).gameObject.SetActive(false);
            //Jetpack tank 1
            jetpack.GetChild(3).gameObject.SetActive(false);
            //Jetpack tank 2
            jetpack.GetChild(4).gameObject.SetActive(false);
        }
    }
}
