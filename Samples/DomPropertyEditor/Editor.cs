//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;

namespace DomPropertyEditorSample
{
    /// <summary>
    /// Creates rootnode and one child of type Orc.</summary>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IInitializable
    {

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_mainform.Shown += (sender, e) =>
                {
                    // create root node.
                    var rootNode = new DomNode(Schema.gameType.Type, Schema.gameRootElement);
                    rootNode.SetAttribute(Schema.gameType.nameAttribute, "Game");

                    // create Orc game object and add it to rootNode.
                    var orc = CreateOrc();
                    rootNode.GetChildList(Schema.gameType.gameObjectChild).Add(orc);

                    // add a child Orc.
                    var orcChildList = orc.GetChildList(Schema.orcType.orcChild);
                    orcChildList.Add(CreateOrc("Child Orc1"));

                    rootNode.InitializeExtensions();

                    // set active context and select orc object.
                    m_contextRegistry.ActiveContext = rootNode;
                    var edContext = rootNode.Cast<GameEditingContext>();
                    edContext.Set(orc);
                };
        }

        #endregion

        /// <summary>
        /// Helper method to create instance of orcType.</summary>
        private static DomNode CreateOrc(string name = "Orc")
        {
            var orc = new DomNode(Schema.orcType.Type);
            orc.SetAttribute(Schema.orcType.nameAttribute, name);
            orc.SetAttribute(Schema.orcType.TextureRevDateAttribute, DateTime.Now);
            orc.SetAttribute(Schema.orcType.resourceFolderAttribute,System.Windows.Forms.Application.StartupPath);
            orc.SetAttribute(Schema.orcType.skinColorAttribute, System.Drawing.Color.DarkGray.ToArgb());
            orc.SetAttribute(Schema.orcType.healthAttribute, 80);
            var armorList = orc.GetChildList(Schema.orcType.armorChild);

            armorList.Add(CreateArmor("Iron breast plate",20,300));

            var clubList = orc.GetChildList(Schema.orcType.clubChild);
            clubList.Add(CreateClub(true, 20, 30));

            return orc;
        }

        private static DomNode CreateArmor(string name, int defense, int price)
        {
            var armor = new DomNode(Schema.armorType.Type);
            armor.SetAttribute(Schema.armorType.nameAttribute, name);
            armor.SetAttribute(Schema.armorType.defenseAttribute, defense);
            armor.SetAttribute(Schema.armorType.priceAttribute, price);
            return armor;
        }

        private static DomNode CreateClub(bool hasSpikes, int damage, float weight)
        {
            var club = new DomNode(Schema.clubType.Type);
            club.SetAttribute(Schema.clubType.spikesAttribute, hasSpikes);
            club.SetAttribute(Schema.clubType.DamageAttribute, damage);
            club.SetAttribute(Schema.clubType.wieghtAttribute, weight);
            return club;
        }
    
    
        [Import(AllowDefault = false)]
        private MainForm m_mainform = null; //initialize to null to avoid incorrect compiler warning

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry = null; //initialize to null to avoid incorrect compiler warning
    }   
}
