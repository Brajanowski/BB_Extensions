using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BB_Extensions.MaterialEditor {
  // NOTE(Brajan): ***A means that is output, ***B input
  [System.Serializable]
  public class Connection  {
    [SerializeField]
    public int nodeA = -1;
    [SerializeField]
    public int nodeB = -1;
    [SerializeField]
    public int nodeASlot = -1;
    [SerializeField]
    public int nodeBSlot = -1;

    public Connection(int nodeA = -1, int nodeB = -1, int nodeASlot = -1, int nodeBSlot = -1) {
      this.nodeA = nodeA;
      this.nodeB = nodeB;
      this.nodeASlot = nodeASlot;
      this.nodeBSlot = nodeBSlot;
    }
  }
}
