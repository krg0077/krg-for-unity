﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace KRG
{
    [System.Serializable]
    public sealed class FrameSequence
    {
        public const int VERSION = 3;

        class FrameCommand
        {
            public bool isNumber, isExtender, isRange, isSeparator; // TODO: make this an enum
            public int number, times = 1;
        }

        // SERIALIZED FIELDS

        // TODO: display this information in the inspector
        // IMPORTANT: frame sequences can't use both "Frames" and "From/To Frame"
        // in the same animation; they must be all one or all the other

        [SerializeField, HideInInspector]
        [FormerlySerializedAs("m_serializedVersion")]
        private int _serializedVersion = VERSION;

        [SerializeField]
        [Tooltip("An optional name you may give this frame sequence.")]
        [FormerlySerializedAs("m_name")]
        private string _name = default;

        [SerializeField]
        [Enum(typeof(FrameSequenceAction))]
        private List<int> _preSequenceActions = default;

        [SerializeField]
        [Tooltip("Commas seperate frames/groups. 1-3-1 means 1,2,3,2,1. 1-3x2-1 means 1-3,3-1 means 1,2,3,3,2,1.")]
        private string _frames = default;

        [SerializeField, ReadOnly]
        [Tooltip("Count of frames in a single playthrough of this sequence.")]
#pragma warning disable IDE0052 // Remove unread private members
        private int _frameCount;
#pragma warning restore IDE0052 // Remove unread private members

        [SerializeField, ReadOnly]
        [Tooltip("This is how the code sees your frames input.")]
#pragma warning disable IDE0052 // Remove unread private members
        private string _interpretedFrames;
#pragma warning restore IDE0052 // Remove unread private members

        [SerializeField, HideInInspector]
        private List<int> _frameList = new List<int>();

        [SerializeField]
        [Tooltip("Count of playthroughs, or \"loops\", of this sequence.")]
        [FormerlySerializedAs("m_playCount")]
        private RangeInt _playCount = new RangeInt();

        // SOON TO BE DEPRECATED SERIALIZED FIELDS

        [Header("Deprecated")]

        [SerializeField]
        [Tooltip("Soon to be deprecated. Use with caution.")]
        [FormerlySerializedAs("m_fromFrame")]
        private RangeInt _fromFrame = new RangeInt();

        [SerializeField]
        [Tooltip("Soon to be deprecated. Use with caution.")]
        [FormerlySerializedAs("m_toFrame")]
        private RangeInt _toFrame = new RangeInt();

        // OBSOLETE SERIALIZED FIELDS

        [HideInInspector, SerializeField]
        [FormerlySerializedAs("m_from")]
        private int _from = default;

        [HideInInspector, SerializeField]
        [FormerlySerializedAs("m_to")]
        private int _to = default;

        [HideInInspector, SerializeField]
        [FormerlySerializedAs("m_doesCallCode")]
        private bool _doesCallCode = false;

        [HideInInspector, SerializeField]
        private int _preSequenceAction = default;

        // FIELDS: PRIVATE / ConvertFramesToFrameList

        private Queue<FrameCommand> _frameCommands = new Queue<FrameCommand>();

        private StringBuilder _number;

        // SHORTCUT PROPERTIES

        public string Name => _name;

        public ReadOnlyCollection<int> FrameList => _frameList.AsReadOnly();

        public int FromFrame => _fromFrame.randomValue;

        public bool FromFrameMinInclusive => _fromFrame.minInclusive;

        public bool FromFrameMaxInclusive => _fromFrame.maxInclusive;

        public int FromFrameMinValue => _fromFrame.minValue;

        public int FromFrameMaxValue => _fromFrame.maxValue;

        public int ToFrame => _toFrame.randomValue;

        public bool ToFrameMinInclusive => _toFrame.minInclusive;

        public bool ToFrameMaxInclusive => _toFrame.maxInclusive;

        public int ToFrameMinValue => _toFrame.minValue;

        public int ToFrameMaxValue => _toFrame.maxValue;

        public int PlayCount => _playCount.randomValue;

        public bool PlayCountMinInclusive => _playCount.minInclusive;

        public bool PlayCountMaxInclusive => _playCount.maxInclusive;

        public int PlayCountMinValue => _playCount.minValue;

        public int PlayCountMaxValue => _playCount.maxValue;

        public List<int> PreSequenceActions => _preSequenceActions;

        // METHODS: PUBLIC

        public void OnValidate()
        {
            // initialization
            if (_playCount.minValue == 0 && _playCount.maxValue == 0)
            {
                _playCount.minValue = 1;
            }

            // migrate deprecated values
            while (_serializedVersion < VERSION)
            {
                if (_serializedVersion == 0)
                {
                    if (_from > 0)
                    {
                        _fromFrame.minValue = _from;
                        _fromFrame.maxValue = _from;
                        _from = 0;
                    }
                    if (_to > 0)
                    {
                        _toFrame.minValue = _to;
                        _toFrame.maxValue = _to;
                        _to = 0;
                    }
                }
                else if (_serializedVersion == 1)
                {
                    if (_doesCallCode)
                    {
                        _name += " [DOES CALL CODE]";
                    }
                    _doesCallCode = false;
                }
                else if (_serializedVersion == 2)
                {
                    if (_preSequenceAction != 0)
                    {
                        if (_preSequenceActions == null)
                        {
                            _preSequenceActions = new List<int>(1);
                        }
                        _preSequenceActions.Add(_preSequenceAction);
                    }
                    _preSequenceAction = 0;
                }
                ++_serializedVersion;
            }

            // real validation
            _fromFrame.minValue = Mathf.Max(1, _fromFrame.minValue);
            _toFrame.minValue = Mathf.Max(_fromFrame.maxValue, _toFrame.minValue);
            _playCount.minValue = Mathf.Max(0, _playCount.minValue);

            ConvertFramesToFrameList();
        }

        // METHODS: ConvertFramesToFrameList

        private void ConvertFramesToFrameList()
        {
            _frameList.Clear();
            _frameCommands.Clear();
            _number = new StringBuilder();
            _frames = _frames?.Trim() ?? "";
            if (_frames == "") return;
            char c;
            for (int i = 0; i < _frames.Length; ++i)
            {
                c = _frames[i];
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        _number.Append(c);
                        break;
                    case 'x':
                        FlushNumberToFrameCommands();
                        _frameCommands.Enqueue(new FrameCommand { isExtender = true });
                        break;
                    case 't': // to
                    case '-':
                        FlushNumberToFrameCommands();
                        _frameCommands.Enqueue(new FrameCommand { isRange = true });
                        break;
                    case ',':
                        FlushNumberToFrameCommands();
                        _frameCommands.Enqueue(new FrameCommand { isSeparator = true });
                        break;
                }
            }
            if (_number.Length > 0) FlushNumberToFrameCommands();
            ProcessFrameCommandExtenders();
            ProcessFrameCommandRanges();
            _frameCount = _frameList.Count;
            _interpretedFrames = ListToString(_frameList);
        }

        private void FlushNumberToFrameCommands()
        {
            if (_number.Length > 0)
            {
                _frameCommands.Enqueue(new FrameCommand { isNumber = true, number = int.Parse(_number.ToString()) });
                _number = new StringBuilder();
            }
            else
            {
                Error("Did you forget a number before/after a symbol?"
                + " Number string is null or empty in AddNumberToCommands.");
            }
        }

        private void ProcessFrameCommandExtenders()
        {
            var q = _frameCommands; // the original queue of frame commands
            _frameCommands = new Queue<FrameCommand>(); // the new, processed queue of frame commands
            FrameCommand prev = null, curr, next; // previous, current, next
            while (q.Count > 0)
            {
                curr = q.Dequeue();
                if (curr.isExtender)
                {
                    if (IsBinaryOperator(prev, q, out next))
                    {
                        prev.times *= next.number;
                    }
                }
                else
                {
                    // this will normally be done first, before curr.isExtender
                    _frameCommands.Enqueue(curr);
                    prev = curr;
                }
            }
        }

        private void ProcessFrameCommandRanges()
        {
            var q = _frameCommands; // the original queue of frame commands
            // this is the last process, so we're gonna affect the original queue this time
            FrameCommand prev = null, curr, next; // previous, current, next
            while (q.Count > 0)
            {
                curr = q.Dequeue();
                if (curr.isRange)
                {
                    if (IsBinaryOperator(prev, q, out next))
                    {
                        AddFramesToList(prev, next);
                        prev = next;
                    }
                }
                else if (curr.isNumber)
                {
                    // this will normally be done first, before curr.isRange or curr.isSeparator
                    AddFrameToList(curr);
                    prev = curr;
                }
                else if (curr.isSeparator)
                {
                    // do nothing
                    prev = null;
                }
                else
                {
                    Error("Unrecognized symbol. Should only be a number,"
                    + " an extender (x), a range dash (-), or a separator comma (,).");
                }
            }
        }

        private static bool IsBinaryOperator(FrameCommand prev, Queue<FrameCommand> q, out FrameCommand next)
        {
            next = null;
            if (q.Count == 0)
            {
                Error("Missing right operator at end.");
                return false;
            }
            if (!q.Peek().isNumber) // peek at next, but don't dequeue it yet
            {
                Error("Missing right operator.");
                return false;
            }
            next = q.Dequeue(); // dequeue next, and set _out_ parameter
            if (prev == null)
            {
                Error("Missing left operator at beginning.");
                return false;
            }
            if (!prev.isNumber)
            {
                Error("Missing left operator.");
                return false;
            }
            return true;
        }

        private void AddFramesToList(FrameCommand fromEx, FrameCommand toIncl) // _from_ exclusive, _to_ inclusive
        {
            if (fromEx.number == toIncl.number)
            {
                Error("Same _from_ and _to_. Ignoring second number. Use a comma if you want it twice.");
                return;
            }
            // add all the numbers between _from_ and _to_
            if (fromEx.number < toIncl.number)
            {
                for (int i = fromEx.number + 1; i < toIncl.number; ++i) _frameList.Add(i);
            }
            else
            {
                for (int i = fromEx.number - 1; i > toIncl.number; --i) _frameList.Add(i);
            }
            // and then add _to_ using its specified _times_ (this way we can process e.g. 1-5x0 as 1,2,3,4)
            AddFrameToList(toIncl);
        }

        private void AddFrameToList(FrameCommand c)
        {
            while (c.times-- > 0) _frameList.Add(c.number);
        }

        private static void Error(string message)
        {
            // TODO: display this information in the inspector
            G.U.Err("Error in FrameSequence. {0}", message);
        }

        private static string ListToString(List<int> list)
        {
            string s = "";
            for (int i = 0; i < list.Count; ++i)
            {
                s += list[i] + ",";
            }
            return s.TrimEnd(',');
        }
    }
}
