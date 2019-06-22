using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Sends command to assign mission to group of uavs. Group is only temporary.
    /// Contains commands which will be send (if they are supported).
    /// </summary>
    class CommandSetMissionToGroup : Command
    {

        public string GroupId;
	    public string MissionId;
        public Dictionary<Commands, List<Command>> Cmds;
        private List<Commands> m_SupportedCommands;

        public CommandSetMissionToGroup(Commands id) : base(id)
        {
            InitCollections();
        }

        public CommandSetMissionToGroup(Commands id, string groupID, string missionID, List<Command> commandList) : this(id)
        {
            GroupId = groupID;
            MissionId = missionID;

            foreach (Command cmds in commandList)
            {
                List<Command> list;
                if (Cmds.TryGetValue(cmds.COMMAND_ID, out list))
                {
                    list.Add(cmds);
                }
                else
                {
                    UnityEngine.Debug.LogError("Unsupported command: " + cmds);
                }
            }
        }

        private void InitCollections()
        {
            m_SupportedCommands = new List<Commands>();
            m_SupportedCommands.Add(Commands.CommandSetSurveillanceArea);
            m_SupportedCommands.Add(Commands.CommandSetTrackingTargets);
            m_SupportedCommands.Add(Commands.CommandSetWaypoints);

            Cmds = new Dictionary<Commands, List<Command>>();

            foreach (Commands cmds in m_SupportedCommands)
            {
                Cmds.Add(cmds, new List<Command>());
            }
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(GroupId, output);
            Convertor.WriteString(MissionId, output);

            foreach (Commands cmds in m_SupportedCommands)
            {
                List<Command> list;
                Cmds.TryGetValue(cmds, out list);
                Convertor.WriteCommandList(list, output);
            }
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            GroupId = Convertor.ReadString(input, next, out next);
            MissionId = Convertor.ReadString(input, next, out next);

            foreach (Commands cmds in m_SupportedCommands)
            {
                List<Command> list;
                Cmds.TryGetValue(cmds, out list);
                Cmds[cmds] = Convertor.ReadCommandList(input, next, out next, cmds);
            }
        }

    }
}
