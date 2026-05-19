using UnityEngine;

[System.Serializable]
public class Phase1RequestData
{
    public string playerId; // "host" or "client"
    public string data;
}

[System.Serializable]
public class Phase1ResponseData
{
    public bool success;
    public string message;
}

[System.Serializable]
public class FusionRequestData
{
    public string hostChars;
    public string clientChars;
}

[System.Serializable]
public class FusionResponseData
{
    public bool success;
    public string fusionResult;
    public string message;
}
