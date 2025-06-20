using RimWorld;
using UnityEngine;
using Verse;

namespace FrankWilco.RimWorld
{
    public class Designator_Ruler : Designator_Cells
    {
        // public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        protected override DesignationDef Designation => RulerDesignationDefOf.FwRuler;

        // public override bool DragDrawOutline => false;

        public Designator_Ruler()
        {
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            defaultLabel = "DesignatorFwRuler".Translate();
            defaultDesc = "DesignatorFwRulerDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/FwRuler");
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds(Map))
            {
                return false;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            return;
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            GenDraw.DrawNoBuildEdgeLines();
        }
    }
}
