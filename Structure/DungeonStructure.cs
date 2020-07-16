using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// Wrapper around the DungeonStructure tree. Used in the generation process
    /// </summary>
    public class DungeonStructure
    {
        /// <summary>
        /// Root element of the tree
        /// </summary>
        public DungeonNode StartElement { get; }

        /// <summary>
        /// Root element meta data
        /// </summary>
        public NodeMetaData NodeMetaData => StartElement.MetaData;

        /// <summary>
        /// Structure specific meta data
        /// </summary>
        public StructureMetaData StructureMetaData { get; }

        /// <summary>
        /// The original abstract structure this structure was created after
        /// </summary>
        public AbstractDungeonStructure AbstractStructure { get; }

        public DungeonStructure(DungeonNode startElement, StructureMetaData structureMetaData, AbstractDungeonStructure abstractStructure)
        {
            StartElement = startElement;
            AbstractStructure = abstractStructure;
            StructureMetaData = structureMetaData;
        }
    }
}