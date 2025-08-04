using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Context {
    public UtilityAIAgent brain;
    public NavMeshAgent agent;
    public Transform target;
    public Sensor sensor;
        
    readonly Dictionary<string, object> data = new();

    public Context(UtilityAIAgent brain) {
        if (brain == null)
            Debug.Log("<color=red>brain is null</color>");
        this.brain = brain;
        this.agent = brain.Agent;
        this.sensor = brain.Sensor;
    }
        
    public T GetData<T>(string key) => data.TryGetValue(key, out var value) ? (T)value : default;
    public void SetData(string key, object value) => data[key] = value;
}
