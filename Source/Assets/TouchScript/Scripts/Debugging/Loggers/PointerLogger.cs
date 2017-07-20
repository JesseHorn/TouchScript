﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using TouchScript.Debugging.Filters;
using TouchScript.Pointers;
using TouchScript.Utils;

namespace TouchScript.Debugging.Loggers
{
    public class PointerLogger : IPointerLogger
    {

        private const int MIN_POINTER_LIST_SIZE = 1000;

        public int PointerCount
        {
            get
            {
                return pointerCount;
            }
        }


        private int pointerCount = 0;
        private int eventCount = 0;

        private List<PointerData> data = new List<PointerData>(1);
        private List<List<PointerLog>> events = new List<List<PointerLog>>(1);

        public void Log(Pointer pointer, PointerEvent evt)
        {
            var id = checkId(pointer);

            var list = getPointerList(id);
            list.Add(new PointerLog()
            {
                Id = eventCount,
                Tick = DateTime.Now.Ticks,
                PointerId = id,
                Event = evt,
                State = new PointerState()
                {
                    Buttons = pointer.Buttons,
                    Position = pointer.Position,
                    PreviousPosition = pointer.PreviousPosition,
                    Flags = pointer.Flags,
                    Target = pointer.GetPressData().Target,
                    TargetPath = TransformUtils.GetHeirarchyPath(pointer.GetPressData().Target),
                }
            });
            eventCount++;
        }

        public List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null)
        {
            //if (filter == null) 
            return new List<PointerData>(data);
        }

        public List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null)
        {
            if (id < 0 || id >= pointerCount)
                return new List<PointerLog>();

            List<PointerLog> list = events[id];
            if (filter == null)
                return new List<PointerLog>(list);

            var count = list.Count;
            List<PointerLog> filtered = new List<PointerLog>(count);
            for (var i = 0; i < count; i++)
            {
                var item = list[i];
                if (filter.Applies(ref item)) filtered.Add(item);
            }
            return filtered;
        }

        private IList<PointerLog> getPointerList(int id)
        {
            return events[id];
        }

        private int checkId(Pointer pointer)
        {
            var id = pointer.Id;
            if (id > pointerCount) throw new InvalidOperationException("Pointer id desync!");
            else if (id == pointerCount)
            {
                var list = new List<PointerLog>(MIN_POINTER_LIST_SIZE);
                events.Add(list);
                data.Add(new PointerData()
                {
                    Id = id,
                    Type = pointer.Type,
                });
                pointerCount++;
            }

            return id;
        }
    }
}

#endif