using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public class StandardBlock : Block
    {
        public bool centralPillars;

        public GameObject foundation;
        public GameObject ceiling;

        public GameObject nWall;
        public GameObject eWall;
        public GameObject sWall;
        public GameObject wWall;

        public GameObject nePillar;
        public GameObject nwPillar;
        public GameObject sePillar;
        public GameObject swPillar;

        public void CheckConditions()
        {
            bool n = Neighbour(new Vector3Int(+0, +0, +1), out Block nNeighbour);
            bool e = Neighbour(new Vector3Int(+1, +0, +0), out Block eNeighbour);
            bool s = Neighbour(new Vector3Int(+0, +0, -1), out Block sNeighbour);
            bool w = Neighbour(new Vector3Int(-1, +0, +0), out Block wNeighbour);
            bool ne = Neighbour(new Vector3Int(+1, 0, +1), out Block neNeighbour);
            bool nw = Neighbour(new Vector3Int(-1, +0, +1), out Block nwNeighbour);
            bool se = Neighbour(new Vector3Int(+1, 0, -1), out Block seNeighbour);
            bool sw = Neighbour(new Vector3Int(-1, 0, -1), out Block swNeighbour);
            bool t = Neighbour(new Vector3Int(0, +1, 0), out Block tNeighbour);
            bool b = Neighbour(new Vector3Int(0, -1, 0), out Block bNeighbour);

            if (nWall) nWall.SetActive(!n || type != nNeighbour.type);
            if (eWall) eWall.SetActive(!e || type != eNeighbour.type);
            if (sWall) sWall.SetActive(!s || type != sNeighbour.type);
            if (wWall) wWall.SetActive(!w || type != wNeighbour.type);

            if (nePillar) nePillar.SetActive((!n && !e) || (!n && eNeighbour.type != type) || (!e && nNeighbour.type != type) || (n && e && (centralPillars || !ne || neNeighbour.type != type) && nNeighbour.type == type && eNeighbour.type == type));
            if (nwPillar) nwPillar.SetActive((!n && !w) || (!n && wNeighbour.type != type) || (!w && nNeighbour.type != type) || (n && w && (!nw || nwNeighbour.type != type) && nNeighbour.type == type && wNeighbour.type == type));
            if (sePillar) sePillar.SetActive((!s && !e) || (!s && eNeighbour.type != type) || (!e && sNeighbour.type != type) || (s && e && (!se || seNeighbour.type != type) && sNeighbour.type == type && eNeighbour.type == type));
            if (swPillar) swPillar.SetActive((!s && !w) || (!s && wNeighbour.type != type) || (!w && sNeighbour.type != type) || (s && w && (!sw || swNeighbour.type != type) && sNeighbour.type == type && wNeighbour.type == type));

            if (foundation) foundation.SetActive(!b);
            if (ceiling) ceiling.SetActive(!t);
        }
    }
}