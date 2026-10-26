using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CubeInvasion.Prototype;

namespace CubeInvasion.ArtTools
{
    // Editor-only, smooth analytical normals and vertex palette; one material per rig part.
    public sealed class CuteMeshBuilder
    {
        readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        readonly List<Color> colors=new List<Color>();
        readonly List<Vector2> uvs=new List<Vector2>();
        readonly List<int> triangles=new List<int>();
        sealed class Part { public string name,parent;public Vector3 pivot;public CuteMeshBuilder mesh=new CuteMeshBuilder(); }
        readonly List<Part> parts=new List<Part>();
        Part active;
        public float DetailScale { get; set; }=1;
        public static readonly Color Suit=new Color(0,0,0,0),Ink=new Color(.035f,.075f,.14f,1),Cream=new Color(1,.94f,.81f,1),Glow=new Color(.22f,.95f,1,1),Cheek=new Color(1,.46f,.51f,1);
        public void PartAt(string name,Vector3 pivot,string parent=null)
        {
            active=parts.Find(p=>p.name==name);
            if(active==null) { active=new Part {name=name,pivot=pivot,parent=parent};parts.Add(active); }
        }
        public ToyArtLibrary.RigPart[] BuildRig(string name,Func<string,Mesh,Mesh> save)
        {
            var result=new List<ToyArtLibrary.RigPart>();
            foreach(var part in parts)result.Add(new ToyArtLibrary.RigPart {name=part.name,parent=part.parent,pivot=part.pivot,mesh=save(name+"_"+part.name,part.mesh.Build(name+"_"+part.name))});
            return result.ToArray();
        }
        void Vertex(Vector3 p,Vector3 n,Color c,Vector2 uv)
        {
            vertices.Add(p);normals.Add(n.normalized);colors.Add(c);uvs.Add(uv);
            if(active!=null)active.mesh.Vertex(p-active.pivot,n,c,uv);
        }
        void Triangle(int a,int b,int c)
        {
            var cross=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);if(cross.sqrMagnitude<1e-13f)return;
            if(Vector3.Dot(cross,normals[a]+normals[b]+normals[c])<0) {int t=b;b=c;c=t;}
            triangles.Add(a);triangles.Add(b);triangles.Add(c);
            if(active!=null)
            {
                int delta=vertices.Count-active.mesh.vertices.Count;
                active.mesh.Triangle(a-delta,b-delta,c-delta);
            }
        }
        public void Sphere(Vector3 centre,Vector3 size,Color color,int slices=28,int rings=16,Quaternion? rotation=null)
        {
            slices=Mathf.Max(8,Mathf.RoundToInt(slices*DetailScale));rings=Mathf.Max(6,Mathf.RoundToInt(rings*DetailScale));
            var q=rotation??Quaternion.identity;var half=size*.5f;int first=vertices.Count;
            for(int y=0;y<=rings;y++)for(int x=0;x<=slices;x++)
            {
                float v=(float)y/rings,u=(float)x/slices;float t=v*Mathf.PI,a=u*Mathf.PI*2;
                var n=new Vector3(Mathf.Sin(t)*Mathf.Cos(a),Mathf.Cos(t),Mathf.Sin(t)*Mathf.Sin(a));
                Vertex(centre+q*Vector3.Scale(n,half),q*new Vector3(n.x/half.x,n.y/half.y,n.z/half.z),color,new Vector2(u,v));
            }
            Grid(first,slices,rings);
        }
        void Grid(int first,int width,int height)
        {
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            { int a=first+y*(width+1)+x,b=a+1,c=a+width+1,d=c+1;Triangle(a,b,d);Triangle(a,d,c); }
        }
        public void RoundBox(Vector3 centre,Vector3 size,Color color,float round=.25f,int divisions=12,Quaternion? rotation=null)
        {
            divisions=Mathf.Max(4,Mathf.RoundToInt(divisions*DetailScale));
            var q=rotation??Quaternion.identity;var half=size*.5f;float radius=Mathf.Min(size.x,Mathf.Min(size.y,size.z))*Mathf.Clamp(round,.01f,.49f);var inner=half-Vector3.one*radius;
            for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
            {
                int first=vertices.Count,b=(axis+1)%3,c=(axis+2)%3;
                for(int y=0;y<=divisions;y++)for(int x=0;x<=divisions;x++)
                {
                    var p=Vector3.zero;p[axis]=sign*half[axis];p[b]=Mathf.Lerp(-half[b],half[b],(float)x/divisions);p[c]=Mathf.Lerp(-half[c],half[c],(float)y/divisions);
                    var closest=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
                    var normal=(p-closest).normalized;Vertex(centre+q*(closest+normal*radius),q*normal,color,new Vector2((float)x/divisions,(float)y/divisions));
                }
                Grid(first,divisions,divisions);
            }
        }
        public void Torus(Vector3 centre,float radius,float thickness,Color color,Quaternion? rotation=null,int sides=36,int tube=10)
        {
            sides=Mathf.Max(10,Mathf.RoundToInt(sides*DetailScale));tube=Mathf.Max(6,Mathf.RoundToInt(tube*DetailScale));
            var q=rotation??Quaternion.identity;int first=vertices.Count;
            for(int y=0;y<=tube;y++)for(int x=0;x<=sides;x++)
            {
                float a=x*Mathf.PI*2/sides,b=y*Mathf.PI*2/tube;var outward=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                var n=outward*Mathf.Cos(b)+Vector3.forward*Mathf.Sin(b);
                Vertex(centre+q*(outward*radius+n*thickness),q*n,color,new Vector2((float)x/sides,(float)y/tube));
            }
            Grid(first,sides,tube);
        }
        public void Barrel(Vector3 centre,float radius,float length,Color color,Quaternion? rotation=null,int sides=32)
        {
            sides=Mathf.Max(10,Mathf.RoundToInt(sides*DetailScale));
            var q=rotation??Quaternion.identity;float[] z={-.5f,-.43f,.35f,.47f,.5f},r={.78f,1,1,.9f,.72f};int first=vertices.Count;
            for(int ring=0;ring<5;ring++)for(int i=0;i<=sides;i++)
            {
                float a=i*Mathf.PI*2/sides;var radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                int lo=Mathf.Max(0,ring-1),hi=Mathf.Min(4,ring+1);float slope=(r[hi]-r[lo])*radius/((z[hi]-z[lo])*length);
                Vertex(centre+q*(radial*radius*r[ring]+Vector3.forward*z[ring]*length),q*(radial-Vector3.forward*slope),color,new Vector2((float)i/sides,ring*.25f));
            }
            Grid(first,sides,4);
            foreach(int sign in new[]{-1,1})
            {
                int center=vertices.Count;Vertex(centre+q*(Vector3.forward*sign*length*.5f),q*(Vector3.forward*sign),color,Vector2.one*.5f);
                for(int i=0;i<=sides;i++)
                {float a=i*Mathf.PI*2/sides;var radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);Vertex(centre+q*(radial*radius*(sign<0?.78f:.72f)+Vector3.forward*sign*length*.5f),q*(Vector3.forward*sign),color,new Vector2(radial.x,radial.y)*.5f+Vector2.one*.5f);}
                for(int i=0;i<sides;i++)Triangle(center,center+1+i,center+2+i);
            }
        }
        public Mesh Build(string name)
        {
            var mesh=new Mesh {name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.SetUVs(0,uvs);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
    }
}
