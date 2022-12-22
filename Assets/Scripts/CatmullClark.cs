using System;
using System.Collections.Generic;
using UnityEngine;


public class CCMeshData
{
    public List<Vector3> points; // Original mesh points
    public List<Vector4> faces; // Original mesh quad faces
    public List<Vector4> edges; // Original mesh edges
    public List<Vector3> facePoints; // Face points, as described in the Catmull-Clark algorithm
    public List<Vector3> edgePoints; // Edge points, as described in the Catmull-Clark algorithm
    public List<Vector3> newPoints; // New locations of the original mesh points, according to Catmull-Clark
}


public static class CatmullClark
{
   
    // Returns a QuadMeshData representing the input mesh after one iteration of Catmull-Clark subdivision.
    public static QuadMeshData Subdivide(QuadMeshData quadMeshData)
    {
        // Create and initialize a CCMeshData corresponding to the given QuadMeshData
        CCMeshData meshData = new CCMeshData();
        meshData.points = quadMeshData.vertices;
        meshData.faces = quadMeshData.quads;
        meshData.edges = GetEdges(meshData);
        meshData.facePoints = GetFacePoints(meshData);
        meshData.edgePoints = GetEdgePoints(meshData);
        meshData.newPoints = GetNewPoints(meshData);

        // Combine facePoints, edgePoints and newPoints into a subdivided QuadMeshData
        int facePointsStrtIdx = meshData.newPoints.Count;
        Dictionary<Vector2, int> edgesIndices = new Dictionary<Vector2, int>();
        List<Vector3> vertics = getNewVertics(meshData, edgesIndices);
        List<Vector4> faces = getNewFaces(meshData, edgesIndices, facePointsStrtIdx);
        return new QuadMeshData(vertics, faces);
    }

    // Returns a list of the new vertics.
    private static List<Vector3> getNewVertics(CCMeshData meshData, Dictionary<Vector2, int> edgesIndices)
    {
        List<Vector3> vertics = meshData.newPoints;
        foreach (var facePoint in meshData.facePoints){
            vertics.Add(facePoint);
        }
        int edgePointsStrtIdx = vertics.Count;
        for (int i = 0; i < meshData.edgePoints.Count; i++)
        {
            vertics.Add(meshData.edgePoints[i]);
            int p1 = (int) meshData.edges[i].x;
            int p2 = (int) meshData.edges[i].y;
            edgesIndices[orderPoints(p1, p2)] = edgePointsStrtIdx + i;
        }
        return vertics;
    }


    // Returns a list of the new faces.
    private static List<Vector4> getNewFaces(CCMeshData meshData, Dictionary<Vector2, int> edgesIndices, int facePointsStrtIdx)
    {
        List<Vector4> faces = new List<Vector4>();
        for (int i = 0; i < meshData.faces.Count; i++)
        {
            Vector4 curFace = meshData.faces[i];
            int p1 = (int) curFace.x;
            int p2 = (int) curFace.y;
            int p3 = (int) curFace.z;
            int p4 = (int) curFace.w;

            int facePoint = facePointsStrtIdx + i;

            int edgeP1P2 = edgesIndices[orderPoints(p1, p2)];
            int edgeP2P3 = edgesIndices[orderPoints(p2, p3)];
            int edgeP3P4 = edgesIndices[orderPoints(p3, p4)];
            int edgeP4P1 = edgesIndices[orderPoints(p4, p1)];

            faces.Add(new Vector4(p1, edgeP1P2, facePoint, edgeP4P1));
            faces.Add(new Vector4(p2, edgeP2P3, facePoint, edgeP1P2));
            faces.Add(new Vector4(p3, edgeP3P4, facePoint, edgeP2P3));
            faces.Add(new Vector4(p4, edgeP4P1, facePoint, edgeP3P4));
        }
        return faces;
    }


    // given two integers, returns them as a vector2 with where vector2[0] <= vector2[1]
    private static Vector2 orderPoints(int p1, int p2)
    {
        if (p1 > p2) (p1, p2) = (p2, p1);
        return new Vector2(p1, p2);
    }

    // Returns a list of all edges in the mesh defined by given points and faces.
    // Each edge is represented by Vector4(p1, p2, f1, f2)
    // p1, p2 are the edge vertices
    // f1, f2 are faces incident to the edge. If the edge belongs to one face only, f2 is -1
    public static List<Vector4> GetEdges(CCMeshData mesh)
    {
        Dictionary<Vector2, Vector4> dictionary = new Dictionary<Vector2, Vector4>();
        for (int faceNum=0; faceNum < mesh.faces.Count; faceNum++)
        {
            for (int i = 0; i < 4; i++)
            {
                int p1 = (int) mesh.faces[faceNum][i];
                int p2 = (int) mesh.faces[faceNum][(i + 1) % 4];
                Vector2 orderedPoints = orderPoints(p1, p2);
                if (dictionary.ContainsKey(orderedPoints))
                {
                    Vector4 edge = dictionary[orderedPoints];
                    edge.w = faceNum;
                    dictionary[orderedPoints] = edge;
                }
                else
                {
                    Vector4 edge = new Vector4(orderedPoints[0], orderedPoints[1], faceNum, -1);
                    dictionary[orderedPoints] = edge;
                }
            }
        }
        List<Vector4> edges = new List<Vector4>();
        foreach (var edge in dictionary.Values)
        {
            edges.Add(edge);
        }
        return edges;
    }

    // Returns a list of "face points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetFacePoints(CCMeshData mesh)
    {
        List<Vector3> facePoints = new List<Vector3>();
        for (int i = 0; i < mesh.faces.Count; i++)
        {
            Vector3 p1 = mesh.points[(int)mesh.faces[i].x];
            Vector3 p2 = mesh.points[(int)mesh.faces[i].y];
            Vector3 p3 = mesh.points[(int)mesh.faces[i].z];
            Vector3 p4 = mesh.points[(int)mesh.faces[i].w];
            facePoints.Add((p1 + p2 + p3 + p4)/4);
        }
        return facePoints;
    }

    // Returns a list of "edge points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetEdgePoints(CCMeshData mesh)
    {
        List<Vector3> edgesPoints = new List<Vector3>();
        for (int i = 0; i < mesh.edges.Count; i++)
        {
            Vector3 p1 = mesh.points[(int)mesh.edges[i].x];
            Vector3 p2 = mesh.points[(int)mesh.edges[i].y];
            Vector3 f1 = mesh.facePoints[(int)mesh.edges[i].z];
            Vector3 f2 = mesh.facePoints[(int)mesh.edges[i].w];
            edgesPoints.Add((p1 + p2 + f1 + f2)/4);
        }
        return edgesPoints;
    }

    // Returns a list of new locations of the original points for the given CCMeshData, as described in the CC algorithm 
    public static List<Vector3> GetNewPoints(CCMeshData mesh)
    {
        // each cell i contains the indices of the edges/faces that the i'th vertic belongs to.
        List<HashSet<int>> edgesPerPoints = new List<HashSet<int>>();
        List<HashSet<int>> facesPerPoints = new List<HashSet<int>>();

        List<Vector3> newPoints = new List<Vector3>();
        findFacesAndEdges(mesh, edgesPerPoints, facesPerPoints);
        for (int i=0; i < mesh.points.Count; i++)
        {
            int n = edgesPerPoints[i].Count;
            Vector3 f = Vector3.zero;
            Vector3 r = Vector3.zero;
            foreach (var indEdge in edgesPerPoints[i])
            {
                Vector4 curEdge = mesh.edges[indEdge];
                Vector3 edgeMidPoint = (mesh.points[(int) curEdge.x] + mesh.points[(int) curEdge.y]) / 2;
                r += edgeMidPoint;
            }
            foreach (var indFace in facesPerPoints[i])
            {
                Vector3 curFacePoint = mesh.facePoints[indFace];
                f += curFacePoint;
            }
            f /= n;
            r /= n;
            Vector3 finalPoint =  (f + (2 * r) + (n - 3) * mesh.points[i]) / n;
            newPoints.Add(finalPoint);
        }
        return newPoints;
    }

    // for each vertex we find all of the edges and faces that it is part of and add their indices to the relevant list.
    private static void findFacesAndEdges(CCMeshData mesh, List<HashSet<int>> edgesPerPoints, List<HashSet<int>> facesPerPoints)
    {
        int countEdge = 0;
        for (int i = 0; i < mesh.points.Count; i++)
        {
            edgesPerPoints.Add(new HashSet<int>());
            facesPerPoints.Add(new HashSet<int>());
        }
        foreach (var edge in mesh.edges)
        {
            edgesPerPoints[(int) edge.x].Add(countEdge);
            edgesPerPoints[(int) edge.y].Add(countEdge);
            facesPerPoints[(int) edge.x].Add((int) edge.z);
            facesPerPoints[(int) edge.y].Add((int) edge.z);
            if (edge.w != -1) 
            {
                facesPerPoints[(int) edge.x].Add((int) edge.w);
                facesPerPoints[(int) edge.y].Add((int) edge.w);
            }

            countEdge++;
        }
    }
}
