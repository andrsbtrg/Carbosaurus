using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Carbosaurus.Commands
{
    public class AssignMaterial : Command
    {
        public AssignMaterial()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static AssignMaterial Instance { get; private set; }

        public LCA.LandscapeMaterial CurrentMaterial { get; private set; }

        public override string EnglishName => "CBAssignMaterial";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var surfaces = SelectSurfaces();

            
            return Result.Success;
        }

        private List<Surface> SelectSurfaces()
        {
            var gc = new GetObject();
            gc.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            gc.EnablePreSelect(true, true);
            gc.GetMultiple(1, 0);
            if (gc.CommandResult() != Result.Success)
            {
                return null;
            }

            var surfaces = new List<Surface>();
            foreach (var surfaceRef in gc.Objects())
            {
                var surface = surfaceRef.Surface();
                if (surface == null)
                    continue;
                if (surface.IsPlanar() == false)
                    continue;
                surfaces.Add(surface);
            }
            return surfaces;
        }

    }
}