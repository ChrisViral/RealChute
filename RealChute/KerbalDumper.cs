using System.Text;
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

namespace RealChute
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalDumper : MonoBehaviour
    {
        private bool keysPressed
        {
            get
            {
                return (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.P))
                    || (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftShift));
            }
        }

        private void Update()
        {
            if (this.keysPressed)
            {
                PrintKerbalModel();
            }

            if (Input.GetKeyDown(KeyCode.O))
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
            Transform parent = part.transform;
            StringBuilder b = new StringBuilder("[KerbalDumper]: Dumping Kerbal data\n\n");
            b.Append("Kerbal name: ").AppendLine(kerbal.name);
            b.Append("Kerbal profession: ").AppendLine(kerbal.experienceTrait.Title);
            b.Append("Profession level: ").AppendLine(kerbal.experienceLevel.ToString());
            b.AppendLine("\nTransform tree:");
            PrintChildren(parent, b, 0, 0);
            print(b.ToString());
        }

        private void PrintChildren(Transform transform, StringBuilder builder, int index, int offset)
        {
            string tab = "\n";
            for (int i = 0; i < offset; i++)
            {
                tab += "\t";
            }
            builder.Append(tab).AppendFormat("{0}: {1}", index, transform.name);
            /*builder.Append(tab).Append("Components:");
            Component[] c = transform.GetComponents(typeof(Component));
            for (int i = 0; i < c.Length; i++)
            {
                builder.Append(tab).AppendFormat("{0}: {1}, {2}", i, c[i].name, c[i].GetType().Name);
            }*/
            for (int i = 0; i < transform.childCount; i++)
            {
                PrintChildren(transform.GetChild(i), builder, i, offset + 1);
            }
        }

        private void DisableJetpack()
        {
            Transform parent = FlightGlobals.ActiveVessel.Parts[0].transform;
            Transform jetpack = parent.GetChild(5).GetChild(3);
            //EVA decals
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
