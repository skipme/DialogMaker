// -----------------------------------------------------------------------
// <copyright file="Actions.cs" company="">
// Vitaliy Burdenkov
// </copyright>
// -----------------------------------------------------------------------

namespace DialogMaker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;

    /// <summary>
    /// Описание класса
    /// Создан: 5/9/2012 12:03:46 AM
    /// Кем   : USER
    /// </summary>
    public class DCommand
    {
        public DBacklog.PHCommand ComType { get; set; }
        public object EngagedObject1 { get; set; }
        public object EngagedObject2 { get; set; }
        public object EngagedObject3 { get; set; }

        Point Invert2p()
        {
            Point pt = (Point)EngagedObject2;
            pt.X = -pt.X;
            pt.Y = -pt.Y;
            return pt;
        }

        public bool InvertCommand(IDialog dlg)
        {
            bool doOk = true;
            switch (ComType)
            {
                case DBacklog.PHCommand.PhraseAdded:
                    dlg.RemovePhrase((int)EngagedObject2);
                    break;
                case DBacklog.PHCommand.PhraseMove:
                    dlg.MovePhrase((int)EngagedObject1, Invert2p());
                    break;
                case DBacklog.PHCommand.PhraseMoveTree:
                    dlg.MovePhraseTree((int)EngagedObject1, Invert2p());
                    break;
                case DBacklog.PHCommand.PhraseRemove:
                    int pindex = dlg.AddPhrase((Phrase)EngagedObject1);
                    foreach (int cc in (List<int>)EngagedObject2)
                    {
                        dlg.Phrase(cc).PhraseConnectReferences.Add(pindex);
                    }
                    break;
                case DBacklog.PHCommand.TimelineAdd:
                    doOk = false;
                    break;
                case DBacklog.PHCommand.TimelineMove:
                    doOk = false;
                    break;
                case DBacklog.PHCommand.TimelineRemove:
                    dlg.AddTimeline((TimeLine)EngagedObject1);
                    break;
                case DBacklog.PHCommand.setDescription:
                    dlg.SetSelectedText(-1, (int)EngagedObject3, EngagedObject2 as string);
                    break;
                case DBacklog.PHCommand.setLabel:
                    dlg.SetSelectedLabel((int)EngagedObject3, EngagedObject2 as string);
                    break;
                case DBacklog.PHCommand.PhraseConnected:
                    dlg.RemoveConnection((int)EngagedObject1, (int)EngagedObject2);
                    break;
                case DBacklog.PHCommand.PhraseConnectionLost:
                    dlg.Connect((int)EngagedObject1, (int)EngagedObject2);
                    break;
                default:
                    Console.WriteLine("unknown undo type: {0}", ComType);
                    doOk = false;
                    break;
            }
            return doOk;
        }
        //public void RepeatCommand(IDialog dlg)
        //{
        //    switch (ComType)
        //    {
        //        case DBacklog.PHCommand.PhraseAdded:
        //            dlg.AddPhrase((Phrase)EngagedObject1);
        //            break;
        //        case DBacklog.PHCommand.PhraseMove:
        //            dlg.MovePhrase((int)EngagedObject1, (Point)EngagedObject2);
        //            break;
        //        case DBacklog.PHCommand.PhraseMoveTree:
        //            dlg.MovePhraseTree((int)EngagedObject1, (Point)EngagedObject2);
        //            break;
        //        case DBacklog.PHCommand.PhraseRemove:
        //            dlg.RemovePhrase((int)EngagedObject2);
        //            break;
        //        case DBacklog.PHCommand.TimelineAdd:
        //            break;
        //        case DBacklog.PHCommand.TimelineMove:
        //            break;
        //        case DBacklog.PHCommand.TimelineRemove:
        //            dlg.AddTimeline((TimeLine)EngagedObject1);
        //            break;
        //        case DBacklog.PHCommand.setDescription:
        //            dlg.SetSelectedText(-1, (int)EngagedObject3, EngagedObject2 as string);
        //            break;
        //        case DBacklog.PHCommand.setLabel:
        //            dlg.SetSelectedLabel((int)EngagedObject3, EngagedObject2 as string);
        //            break;
        //        case DBacklog.PHCommand.PhraseConnected:
        //            dlg.RemoveConnection((int)EngagedObject1, (int)EngagedObject2);
        //            break;
        //        case DBacklog.PHCommand.PhraseConnectionLost:
        //            dlg.Connect((int)EngagedObject1, (int)EngagedObject2);
        //            break;
        //        default:
        //            Console.WriteLine("unknown undo type: {0}", ComType);
        //            break;
        //    }
        //}
    }
}
