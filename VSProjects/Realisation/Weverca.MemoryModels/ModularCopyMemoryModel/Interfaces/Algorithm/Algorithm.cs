﻿using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Generec factory interface for algorithm type.
    /// 
    /// Algorithm factory always creates new instance of algorithm. Algorithm is used only for
    /// single job. Parameters for run of algorithm will be cpecified within algorithm public
    /// interface.
    /// </summary>
    /// <typeparam name="T">Type of algorithm to create.</typeparam>
    public interface IAlgorithmFactory<T>
    {
        /// <summary>
        /// Creates new instance of algorithm.
        /// </summary>
        /// <returns>New instance of algorithm.</returns>
        T CreateInstance();
    }

    /// <summary>
    /// Defines set of assign algorithms for Modular copy Memory Model.
    /// 
    /// All of these algorithms updates the structure or data.
    /// </summary>
    public interface IAssignAlgorithm
    {
        /// <summary>
        /// Assigns specified value to memory locations on specified path within the specified
        /// snapshot. 
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="path">The path.</param>
        /// <param name="value">The value.</param>
        /// <param name="forceStrongWrite">if set to <c>true</c> algorithm provides MUST write for all memory locations.</param>
        void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite);

        /// <summary>
        /// Creates alliases between memory locations defined in source and target paths.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="sourcePath">The source path.</param>
        void AssignAlias(Snapshot snapshot, MemoryPath targetPath, MemoryPath sourcePath);

        /// <summary>
        /// Writes the values to specified location without copy or inner merge.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="path">The path.</param>
        /// <param name="value">The value.</param>
        void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value);
    }

    /// <summary>
    /// Defines set of read algorithms for Modular copy Memory Model.
    /// 
    /// User of this algorithm has to call Read method at first. This method collects memory
    /// indexes and memory entyry from the given path. Then user can use any of get methods to get
    /// desired information.
    /// 
    /// All of these algorithms do not update structure or data.
    /// </summary>
    public interface IReadAlgorithm
    {
        /// <summary>
        /// Reads the values from indexes on specified path within the specified snapshot.
        /// 
        /// This method has to be called once before get methods.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="path">The path.</param>
        void Read(Snapshot snapshot, MemoryPath path);

        /// <summary>
        /// Reads the given values within the specified snapshot.
        /// 
        /// This method has to be called once before get methods.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="values">The values.</param>
        void Read(Snapshot snapshot, MemoryEntry values);

        /// <summary>
        /// Determines whether read locations are defined.
        /// </summary>
        /// <returns>True whether read locations are defined.</returns>
        bool IsDefined();

        /// <summary>
        /// Gets the list of read values.
        /// </summary>
        /// <returns>List of read values.</returns>
        MemoryEntry GetValue();

        /// <summary>
        /// Gets the collection of fields from objects on read locations.
        /// </summary>
        /// <returns>The collection of fields from objects on read locations.</returns>
        IEnumerable<VariableIdentifier> GetFields();

        /// <summary>
        /// Gets the collection of indexes from arrays on read locations.
        /// </summary>
        /// <returns>The collection of indexes from arrays on read locations.</returns>
        IEnumerable<MemberIdentifier> GetIndexes();

        /// <summary>
        /// Gets the collection of methods from objects on read locations.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>The collection of methods from objects on read locations.</returns>
        IEnumerable<FunctionValue> GetMethod(QualifiedName methodName);

        /// <summary>
        /// Gets the collection of types from objects on read locations.
        /// </summary>
        /// <returns>The collection of types from objects on read locations.</returns>
        IEnumerable<TypeValue> GetObjectType();
    }

    /// <summary>
    /// Defines set of commit algorithms for Modular copy Memory Model.
    /// 
    /// These algorithms are used to compare two structures or data sets to get the information
    /// whether structures or data are different. This is used at the end of transaction to compare
    /// old and new snapshot state.
    /// 
    /// To propper function user has to set compared structure and data first. Then use one of
    /// commit method. The last method is to get information wherher structures or data differs or not.
    /// 
    /// These algorithms can modify data or structure.
    /// </summary>
    public interface ICommitAlgorithm
    {
        /// <summary>
        /// Sets the structures to compare.
        /// </summary>
        /// <param name="newStructure">The new structure.</param>
        /// <param name="oldStructure">The old structure.</param>
        void SetStructure(ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure);

        /// <summary>
        /// Sets the data to compare.
        /// </summary>
        /// <param name="newData">The new data.</param>
        /// <param name="oldData">The old data.</param>
        void SetData(ISnapshotDataProxy newData, ISnapshotDataProxy oldData);

        /// <summary>
        /// Commits the snapshot. Compares structure and sets of data for modifications.
        /// Also searchs for memory entries which contains more values than specified simplifyLimit
        /// and simplifies these entries.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        void CommitAndSimplify(Snapshot snapshot, int simplifyLimit);

        /// <summary>
        /// Commits the snapshot. Compares structure and sets of data for modifications.
        /// Modified data are also widened using snapshot memory assistant.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        void CommitAndWiden(Snapshot snapshot, int simplifyLimit);

        /// <summary>
        /// Determines whether this new structure or data differs or not.
        /// </summary>
        /// <returns>True whether this new structure or data differs.</returns>
        bool IsDifferent();
    }

    /// <summary>
    /// Defines set of merge algorithms for Modular copy Memory Model.
    /// 
    /// These algorithms are used to join memory snapshots together.
    /// 
    /// These algorithms creates new data and structure.
    /// </summary>
    public interface IMergeAlgorithm
    {
        /// <summary>
        /// Extends the specified snapshot by copying data and structure of the source snapshot
        /// to extended snapshot.
        /// </summary>
        /// <param name="extendedSnapshot">The extended snapshot.</param>
        /// <param name="sourceSnapshot">The source snapshot.</param>
        void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot);

        /// <summary>
        /// Data of the specified snapshot are merged together and inserted to the specified
        /// snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="snapshots">The snapshots.</param>
        void Merge(Snapshot snapshot, List<Snapshot> snapshots);

        /// <summary>
        /// Data of the specified snapshot are merged together and inserted to the specified
        /// snapshot and local level of memory stack is closed.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="snapshots">The snapshots.</param>
        void MergeWithCall(Snapshot snapshot, List<Snapshot> snapshots);

        /// <summary>
        /// Merges data of the given data entry and stores it within given temporary index.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="temporaryIndex">Index of the temporary.</param>
        /// <param name="dataEntry">The data entry.</param>
        void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry);

        /// <summary>
        /// Gets the merged structure.
        /// </summary>
        /// <returns>The merged structure.</returns>
        ISnapshotStructureProxy GetMergedStructure();

        /// <summary>
        /// Gets the merged data.
        /// </summary>
        /// <returns>The merged data.</returns>
        ISnapshotDataProxy GetMergedData();
    }

    /// <summary>
    /// Defines set of memory algorithms for Modular copy Memory Model.
    /// 
    /// These algorithms modifies data and structure.
    /// </summary>
    public interface IMemoryAlgorithm
    {
        /// <summary>
        /// Copies the memory values between specified locations.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="isMust">if set to <c>true</c> the copy is must, otherwise may.</param>
        void CopyMemory(Snapshot snapshot, MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust);

        /// <summary>
        /// Deletes all data stored in given memory index from data and structure.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="index">The index.</param>
        void DestroyMemory(Snapshot snapshot, MemoryIndex index);
    }
}