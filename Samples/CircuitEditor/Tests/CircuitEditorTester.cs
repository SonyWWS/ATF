//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample.Tests
{
    /// <summary>
    /// Class for testing Circuit Editor</summary>
    public class CircuitEditorTester
    {
        /// <summary>
        /// Create circuit DOM hierarchy programmatically</summary>
        /// <param name="schemaLoader">Schema loader</param>
        /// <returns>Tree root DomNode</returns>
        static public DomNode CreateTestCircuitProgrammatically(SchemaLoader schemaLoader)
        {
            var rootNode = new DomNode(Schema.circuitDocumentType.Type, Schema.circuitRootElement);        
            // create an empty root prototype folder( required child by schema)
            rootNode.SetChild(
                Schema.circuitDocumentType.prototypeFolderChild,
                new DomNode(Schema.prototypeFolderType.Type));
            
            var circuit = rootNode.Cast<Circuit>();

            var inputFiles = new DomNode(Schema.groupType.Type).Cast<Group>();
            inputFiles.Id = "groupInputFiles";
            inputFiles.Name = "Input Files".Localize();
            inputFiles.Bounds = new Rectangle(64, 96, 0, 0); // set node location, size will be auto-computed
 
            var firstWavgGroup = new DomNode(Schema.groupType.Type).Cast<Group>();
            firstWavgGroup.Id = "first.Wav";
            firstWavgGroup.Name = "first".Localize("as in, 'the first file'") + ".wav";

            var buttonType = schemaLoader.GetNodeType(Schema.NS + ButtonTypeName);
            var button1 = new DomNode(buttonType).Cast<Module>();
            button1.Id = "button1";
            button1.Bounds = new Rectangle(0, 0, 0, 0);

            var button2 = new DomNode(buttonType).Cast<Module>();
            button2.Bounds = new Rectangle(0, 64, 0, 0);
            button2.Id = "button2";

            firstWavgGroup.Elements.Add(button1);
            firstWavgGroup.Elements.Add(button2);
            firstWavgGroup.Expanded = true;
            firstWavgGroup.Update();


            var secondWavgGroup = new DomNode(Schema.groupType.Type).Cast<Group>();
            secondWavgGroup.Id = "second.Wav";
            secondWavgGroup.Name = "second".Localize("as in, 'the second file'") + ".wav";

            var button3 = new DomNode(buttonType).Cast<Module>();
            button3.Id = "button3";
            button3.Bounds = new Rectangle(0, 0, 0, 0);

            var button4 = new DomNode(buttonType).Cast<Module>();
            button4.Bounds = new Rectangle(0, 64, 0, 0);
            button4.Id = "button4";

            secondWavgGroup.Elements.Add(button3);
            secondWavgGroup.Elements.Add(button4);
            secondWavgGroup.Expanded = true;
            secondWavgGroup.Update();
            secondWavgGroup.Bounds = new Rectangle(0, 224, 0, 0);
  
            inputFiles.Elements.Add(firstWavgGroup);
            inputFiles.Elements.Add(secondWavgGroup);
            inputFiles.Update();
            inputFiles.Expanded = true;

            circuit.Elements.Add(inputFiles);


            var structure = new DomNode(Schema.groupType.Type).Cast<Group>();
            structure.Id = "structure".Localize("this is the name of a group of circuit elements; the name is arbitrary");
            structure.Name = "structure".Localize("this is the name of a group of circuit elements; the name is arbitrary");
            structure.Bounds = new Rectangle(352, 96, 0, 0); 
 

            var subStream0 = new DomNode(Schema.groupType.Type).Cast<Group>();
            subStream0.Id = "subStream0".Localize("this is the name of a group of circuit elements; the name is arbitrary");
            subStream0.Name = "sub-stream 0".Localize("this is the name of a group of circuit elements; the name is arbitrary");

            var lightType = schemaLoader.GetNodeType(Schema.NS + LightTypeName);

            var light1 = new DomNode(lightType).Cast<Module>();
            light1.Id = "light1";
            light1.Bounds = new Rectangle(0, 0, 0, 0);

            var light2 = new DomNode(lightType).Cast<Module>();
            light2.Id = "light2";
            light2.Bounds = new Rectangle(0, 64, 0, 0);

            var light3 = new DomNode(lightType).Cast<Module>();
            light3.Id = "light3";
            light3.Bounds = new Rectangle(0, 128, 0, 0);

            var light4 = new DomNode(lightType).Cast<Module>();
            light4.Id = "light4";
            light4.Bounds = new Rectangle(0, 192, 0, 0);

            var light5 = new DomNode(lightType).Cast<Module>();
            light5.Id = "light5";
            light5.Bounds = new Rectangle(0, 256, 0, 0);

            var light6 = new DomNode(lightType).Cast<Module>();
            light6.Id = "light6";
            light6.Bounds = new Rectangle(0, 320, 0, 0);


            subStream0.Elements.Add(light1);
            subStream0.Elements.Add(light2);
            subStream0.Elements.Add(light3);
            subStream0.Elements.Add(light4);
            subStream0.Elements.Add(light5);
            subStream0.Elements.Add(light6);
            subStream0.Expanded = true;
            subStream0.Update(); // this will generate group pins needed for edge connection


            structure.Elements.Add(subStream0);
            structure.Expanded = true;
            structure.Update();

            circuit.Elements.Add(structure);
			
			// first make all group pins are visible so we can connect them
	        foreach (var groupPin in inputFiles.InputGroupPins)
		        groupPin.Visible = true;
			foreach (var groupPin in inputFiles.OutputGroupPins)
				groupPin.Visible = true;
			foreach (var groupPin in structure.InputGroupPins)
				groupPin.Visible = true;
			foreach (var groupPin in structure.OutputGroupPins)
				groupPin.Visible = true;

			// make some edges between InputFiles & structure
			var connection0 = CircuitAddEdge(inputFiles, 0, structure, 0);
			circuit.Wires.Add(connection0);
			var connection1 = CircuitAddEdge(inputFiles, 1, structure, 1);
			circuit.Wires.Add(connection1);
			var connection2 = CircuitAddEdge(inputFiles, 2, structure, 3);
			circuit.Wires.Add(connection2);
			var connection3 = CircuitAddEdge(inputFiles, 3, structure, 5);
			circuit.Wires.Add(connection3);

            return rootNode;
        }

		private static Wire CircuitAddEdge(Element fromNode, int fromPinIndex, Element toNode, int toPinIndex)
		{
			DomNode domNode = new DomNode(Schema.connectionType.Type);

			Wire connection = domNode.As<Wire>();
			connection.OutputElement = fromNode;
			connection.OutputPin = fromNode.Type.Outputs[fromPinIndex];
			connection.InputElement = toNode;
			connection.InputPin = toNode.Type.Inputs[toPinIndex];
			connection.SetPinTarget();
			connection.Cast<WireStyleProvider<Module, Connection, ICircuitPin>>().EdgeStyle = EdgeStyle.DirectCurve;

			return connection;
		}

        private const string ButtonTypeName = ":buttonType";
        private const string LightTypeName = ":lightType";
    }
}
