namespace Assets.Scripts.Network.Command
{
    public enum Commands : short
    {

        CommandSetWaypoints = 111,
        CommandCreateNoFlyZone = 114,
        CommandRemoveNoFlyZone = 115,
        CommandWaypointCompleted = 119,
        CommandPlanProposal = 121,
        CommandPlanSelection = 122,
        SetSurveillanceArea = 200,
        SetTrackingTarget = 201,
        SetWaypoints = 202,
        Stop = 203,
        StopAll = 204,
        DropCurrentMission = 205,
        Land = 206,
        TakeOff = 207,
        SetAviatorOff = 207,
        SetNoFlyZone = 208,
        SetPolygonArea = 209,
        SetHoldContinue = 210,
        PlanProposal = 222,
        OperatorPlanProposal = 223,
        SetPlan = 223,
        CommandAddUAVToGroup = 300,
        CommandRemoveUAVFromGroup = 301,
        CommandSetMissionToGroup = 302,
        CommandSetSurveillanceArea = 303,
        CommandSetTrackingTargets = 304,
        CommandSetWaypointsToGroup = 305,
        CommandSetPolygonArea = 306,
        InfoUAVAllocation = 400,
        InfoMissionExecution = 401,
        InfoGroundTarget = 402,
        InfoUAVTelemetry = 403,
        InfoUAVTrajectory = 404,
        SetAutoTracking = 511,
        SetPatrolling = 1337

    }
}
