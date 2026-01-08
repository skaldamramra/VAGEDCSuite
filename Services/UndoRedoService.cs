using System;
using System.Collections.Generic;

namespace VAGSuite.Services
{
    /// <summary>
    /// Implementation of IUndoRedoService using two stacks to manage undo/redo history.
    /// </summary>
    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<UndoState> _undoStack;
        private readonly Stack<UndoState> _redoStack;
        private readonly int _maxCapacity;
        
        // Pre-edit state storage for cell editing
        private byte[] _preEditState;
        
        /// <inheritdoc />
        public event EventHandler StateChanged;

        /// <summary>
        /// Represents a single state in the undo/redo history.
        /// </summary>
        private class UndoState
        {
            public byte[] Content { get; set; }
            public string ActionName { get; set; }
            public DateTime Timestamp { get; set; }

            public UndoState(byte[] content, string actionName)
            {
                Content = content ?? throw new ArgumentNullException(nameof(content));
                ActionName = actionName ?? string.Empty;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Creates a new UndoRedoService with default capacity of 50 items.
        /// </summary>
        public UndoRedoService() : this(50)
        {
        }

        /// <summary>
        /// Creates a new UndoRedoService with specified capacity.
        /// </summary>
        /// <param name="maxCapacity">Maximum number of states to keep in each stack</param>
        public UndoRedoService(int maxCapacity)
        {
            _undoStack = new Stack<UndoState>();
            _redoStack = new Stack<UndoState>();
            _maxCapacity = maxCapacity;
        }

        /// <inheritdoc />
        public void PushState(byte[] mapContent, string actionName)
        {
            if (mapContent == null) throw new ArgumentNullException(nameof(mapContent));

            // Clone the content to ensure immutability
            byte[] clonedContent = (byte[])mapContent.Clone();
            
            _undoStack.Push(new UndoState(clonedContent, actionName));
            
            // Enforce capacity limit
            while (_undoStack.Count > _maxCapacity)
            {
                _undoStack.Pop();
            }
            
            // Clear redo stack when new action is performed (standard undo/redo behavior)
            _redoStack.Clear();
            
            OnStateChanged();
        }

        /// <inheritdoc />
        public byte[] Undo(byte[] currentContent)
        {
            if (_undoStack.Count == 0) return null;

            // Push current state to redo stack
            if (currentContent != null)
            {
                byte[] clonedCurrent = (byte[])currentContent.Clone();
                _redoStack.Push(new UndoState(clonedCurrent, "Undo"));
            }

            // Pop from undo stack and return the previous state
            UndoState previousState = _undoStack.Pop();
            OnStateChanged();
            
            return previousState.Content;
        }

        /// <inheritdoc />
        public byte[] Redo(byte[] currentContent)
        {
            if (_redoStack.Count == 0) return null;

            // Push current state to undo stack
            if (currentContent != null)
            {
                byte[] clonedCurrent = (byte[])currentContent.Clone();
                _undoStack.Push(new UndoState(clonedCurrent, "Redo"));
            }

            // Pop from redo stack and return the next state
            UndoState nextState = _redoStack.Pop();
            OnStateChanged();
            
            return nextState.Content;
        }

        /// <inheritdoc />
        public bool CanUndo => _undoStack.Count > 0;

        /// <inheritdoc />
        public bool CanRedo => _redoStack.Count > 0;

        /// <inheritdoc />
        public string NextUndoAction
        {
            get
            {
                if (_undoStack.Count == 0) return string.Empty;
                return "Undo " + _undoStack.Peek().ActionName;
            }
        }

        /// <inheritdoc />
        public string NextRedoAction
        {
            get
            {
                if (_redoStack.Count == 0) return string.Empty;
                return "Redo " + _redoStack.Peek().ActionName;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }

        /// <summary>
        /// Stores the current state before a cell edit begins.
        /// </summary>
        /// <param name="preEditContent">The content to store for potential undo</param>
        public void StorePreEditState(byte[] preEditContent)
        {
            if (preEditContent != null)
            {
                _preEditState = (byte[])preEditContent.Clone();
            }
        }

        /// <summary>
        /// Gets the stored pre-edit state and clears it.
        /// </summary>
        /// <returns>The stored pre-edit state, or null if none was stored</returns>
        public byte[] GetAndClearPreEditState()
        {
            byte[] state = _preEditState;
            _preEditState = null;
            return state;
        }

        /// <summary>
        /// Clears the pre-edit state without returning it.
        /// </summary>
        public void ClearPreEditState()
        {
            _preEditState = null;
        }

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}