using System;

namespace VAGSuite.Services
{
    /// <summary>
    /// Interface for a stack-based Undo/Redo service that manages map content snapshots.
    /// </summary>
    public interface IUndoRedoService
    {
        /// <summary>
        /// Pushes the current state onto the undo stack and clears the redo stack.
        /// </summary>
        /// <param name="mapContent">The byte array content to store</param>
        /// <param name="actionName">Human-readable name of the action being performed</param>
        void PushState(byte[] mapContent, string actionName);

        /// <summary>
        /// Performs an undo operation by popping from the undo stack and pushing current state to redo.
        /// </summary>
        /// <param name="currentContent">The current content that will be pushed to redo stack</param>
        /// <returns>The previous state byte array, or null if undo is not possible</returns>
        byte[] Undo(byte[] currentContent);

        /// <summary>
        /// Performs a redo operation by popping from the redo stack and pushing current state to undo.
        /// </summary>
        /// <param name="currentContent">The current content that will be pushed to undo stack</param>
        /// <returns>The next state byte array, or null if redo is not possible</returns>
        byte[] Redo(byte[] currentContent);

        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Gets a value indicating whether a redo operation is available.
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Gets the name of the next action that will be undone.
        /// </summary>
        string NextUndoAction { get; }

        /// <summary>
        /// Gets the name of the next action that will be redone.
        /// </summary>
        string NextRedoAction { get; }

        /// <summary>
        /// Clears both undo and redo stacks.
        /// </summary>
        void Clear();

        /// <summary>
        /// Stores the current state before a cell edit begins.
        /// </summary>
        /// <param name="preEditContent">The content to store for potential undo</param>
        void StorePreEditState(byte[] preEditContent);

        /// <summary>
        /// Gets the stored pre-edit state and clears it.
        /// </summary>
        /// <returns>The stored pre-edit state, or null if none was stored</returns>
        byte[] GetAndClearPreEditState();

        /// <summary>
        /// Event fired when the state of the stacks changes (for UI updates).
        /// </summary>
        event EventHandler StateChanged;
    }
}