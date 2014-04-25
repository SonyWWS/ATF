using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace UsingDom
{
    /// <summary>
    /// This is a simple demo of basic DOM use in a sample application.
    /// It illustrates loading a schema with a schema loader, 
    /// creating a game using DomNodes and then saving the game data using the schema loader.
    /// It demonstrates creating application data, with and without DOM adapters.
    /// It has no UI, running in a command prompt window.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-Using-Dom-Sample. </summary>
    class Program
    {
        /// <summary>
        /// The main entry point for the application</summary>
        private static void Main(string[] args)
        {
            string ExecutablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                                    
            var gameSchemaLoader = new GameSchemaLoader();
            
           
            DomNode game = null;
            // create game either using DomNode or DomNodeAdapter.
            game = CreateGameUsingDomNode();
            //game = CreateGameUsingDomNodeAdapter();

            Print(game);

            // create directory for data files
            Directory.CreateDirectory(Path.Combine(ExecutablePath, @"data"));
            string filePath = Path.Combine(ExecutablePath, "data\\game.xml");
            var gameUri = new Uri(filePath);
            
            // save game.            
            FileMode fileMode = FileMode.Create;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(gameSchemaLoader.TypeCollection);
                writer.Write(game, stream, gameUri);
            }
        }  
      
        /// <summary>
        /// Creates game by creating DomNodes and not using DOM adapters</summary>
        private static DomNode CreateGameUsingDomNode()
        {                                     
            // Create DOM node of the root type defined by the schema
            DomNode game = new DomNode(GameSchema.gameType.Type, GameSchema.gameRootElement);
            game.SetAttribute(GameSchema.gameType.nameAttribute, "Ogre Adventure II");
            IList<DomNode> childList = game.GetChildList(GameSchema.gameType.gameObjectChild);

            // Add an ogre
            DomNode ogre = new DomNode(GameSchema.ogreType.Type);
            ogre.SetAttribute(GameSchema.ogreType.nameAttribute, "Bill");
            ogre.SetAttribute(GameSchema.ogreType.sizeAttribute, 12);
            ogre.SetAttribute(GameSchema.ogreType.strengthAttribute, 100);
            childList.Add(ogre);


            // Add a dwarf
            DomNode dwarf = new DomNode(GameSchema.dwarfType.Type);
            dwarf.SetAttribute(GameSchema.dwarfType.nameAttribute, "Sally");
            dwarf.SetAttribute(GameSchema.dwarfType.ageAttribute, 32);
            dwarf.SetAttribute(GameSchema.dwarfType.experienceAttribute, 55);
            childList.Add(dwarf);

            // Add a tree
            DomNode tree = new DomNode(GameSchema.treeType.Type);
            tree.SetAttribute(GameSchema.treeType.nameAttribute, "Mr. Oak");
            childList.Add(tree);

            return game;           
        }

        /// <summary>
        /// Creates game using DOM adapters</summary>        
        private static DomNode CreateGameUsingDomNodeAdapter()
        {
            // Create game
            DomNode root = new DomNode(GameSchema.gameType.Type, GameSchema.gameRootElement);
            Game game = root.As<Game>();
            game.Name = "Ogre Adventure II";

            // Add an ogre
            DomNode ogreNode = new DomNode(GameSchema.ogreType.Type);
            Ogre orge = ogreNode.As<Ogre>();
            orge.Name = "Bill";
            orge.Size = 12;
            orge.Strength = 100;
            game.GameObjects.Add(orge);

            // Add a dwarf
            DomNode dwarfNode = new DomNode(GameSchema.dwarfType.Type);
            Dwarf dwarf = dwarfNode.As<Dwarf>();
            dwarf.Name = "Sally";
            dwarf.Age = 32;
            dwarf.Experience = 55;
            game.GameObjects.Add(dwarf);

            // Add a tree
            DomNode treeNode = new DomNode(GameSchema.treeType.Type);
            GameObject tree = treeNode.As<GameObject>();
            tree.Name = "Mr. Oak";
            game.GameObjects.Add(tree);

            return game.DomNode;
        }

        private static void Print(DomNode game)
        {
            Console.WriteLine("Game: {0}", game.GetAttribute(game.Type.GetAttributeInfo("name")));

            foreach (DomNode child in game.Children)
            {
                Console.WriteLine();
                Console.WriteLine("   {0}", child.Type.Name);
                foreach (AttributeInfo attr in child.Type.Attributes)
                    Console.WriteLine("      {0}: {1}",
                            attr.Name,
                            child.GetAttribute(attr));
            }
            Console.WriteLine();
        }
    }
}
