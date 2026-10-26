using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CubeInvasion.Prototype;

namespace CubeInvasion.ArtTools
{
    // Editor-only low-poly kit: chamfered blocks with hard face normals and a linear vertex palette.
    // Shapes are authored in metres above the ground and mapped into the gameplay transform's mesh space.
    public sealed class VoidToyMeshBuilder
    {
        readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        readonly List<Color> colors=new List<Color>();
        readonly List<Vector2> masks=new List<Vector2>();
        readonly List<int> triangles=new List<int>();
        sealed class Part { public string name,parent;public Vector3 pivot;public VoidToyMeshBuilder mesh=new VoidToyMeshBuilder(); }
        readonly List<Part> parts=new List<Part>();
        Part active;
        // Mesh = (design + Offset) / Scale; normals use the inverse-transpose so they read correctly after scaling.
        public Vector3 Scale=Vector3.one,Offset=Vector3.zero;
        // UV0.x: 1 where a runtime-tinted prop takes its MaterialPropertyBlock colour (edge light, gates, beacons).
        public float Tint;
        public int Triangles=>triangles.Count/3;
        public static Color Glow(Color c) {c.a=0;return c;}
        Vector3 Map(Vector3 p)=>new Vector3((p.x+Offset.x)/Scale.x,(p.y+Offset.y)/Scale.y,(p.z+Offset.z)/Scale.z);
        public void PartAt(string name,Vector3 pivot,string parent=null)
        {
            active=parts.Find(p=>p.name==name);
            if(active==null) {active=new Part {name=name,pivot=Map(pivot),parent=parent};parts.Add(active);}
        }
        public ToyArtLibrary.RigPart[] BuildRig(string name,Func<string,Mesh,Mesh> save)
        {
            var result=new List<ToyArtLibrary.RigPart>();
            foreach(var part in parts)result.Add(new ToyArtLibrary.RigPart {name=part.name,parent=part.parent,pivot=part.pivot,mesh=save(name+"_"+part.name,part.mesh.Build(name+"_"+part.name))});
            return result.ToArray();
        }
        public string[] PartNames()=>parts.ConvertAll(p=>p.name).ToArray();
        void Raw(Vector3 p,Vector3 n,Color c,float tint) {vertices.Add(p);normals.Add(n);colors.Add(c);masks.Add(new Vector2(tint,0));}
        void Vertex(Vector3 p,Vector3 n,Color c)
        {
            var mp=Map(p);var mn=Vector3.Scale(n,Scale).normalized;var lc=c.linear;lc.a=c.a;
            Raw(mp,mn,lc,Tint);if(active!=null)active.mesh.Raw(mp-active.pivot,mn,lc,Tint);
        }
        void Triangle(int a,int b,int c)
        {
            var cross=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);if(cross.sqrMagnitude<1e-14f)return;
            if(Vector3.Dot(cross,normals[a]+normals[b]+normals[c])<0) {int t=b;b=c;c=t;}
            triangles.Add(a);triangles.Add(b);triangles.Add(c);
            if(active!=null) {int delta=vertices.Count-active.mesh.vertices.Count;active.mesh.triangles.Add(a-delta);active.mesh.triangles.Add(b-delta);active.mesh.triangles.Add(c-delta);}
        }
        void Polygon(Vector3 centre,Quaternion q,Vector3 normal,Color color,params Vector3[] corners)
        {
            int first=vertices.Count;foreach(var corner in corners)Vertex(centre+q*corner,q*normal,color);
            for(int i=1;i<corners.Length-1;i++)Triangle(first,first+i,first+i+1);
        }
        // 6 faces, 12 edge chamfers, 8 corner facets: 44 triangles with crisp highlights on every edge.
        public void Box(Vector3 centre,Vector3 size,Color color,float bevel=.14f,Quaternion? rotation=null)
        {
            var q=rotation??Quaternion.identity;var h=size*.5f;float b=Mathf.Min(size.x,Mathf.Min(size.y,size.z))*Mathf.Clamp(bevel,.01f,.45f);var i=h-Vector3.one*b;
            for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)
            {
                var d=new Vector3(x,y,z);int k=Mathf.Abs(x)+Mathf.Abs(y)+Mathf.Abs(z);if(k==0)continue;
                var axes=new List<int>();var free=new List<int>();for(int a=0;a<3;a++)(d[a]!=0?axes:free).Add(a);
                Vector3 P(Func<int,float> f)=>new Vector3(f(0),f(1),f(2));
                if(k==1)
                {
                    int a=axes[0],u=free[0],v=free[1];
                    Vector3 C(float su,float sv)=>P(j=>j==a?d[a]*h[a]:j==u?su*i[u]:sv*i[v]);
                    Polygon(centre,q,d,color,C(-1,-1),C(1,-1),C(1,1),C(-1,1));
                }
                else if(k==2)
                {
                    int a=axes[0],c=axes[1],t=free[0];
                    Vector3 C(bool outerA,float st)=>P(j=>j==t?st*i[t]:j==a?d[a]*(outerA?h[a]:i[a]):d[c]*(outerA?i[c]:h[c]));
                    Polygon(centre,q,d.normalized,color,C(true,-1),C(true,1),C(false,1),C(false,-1));
                }
                else Polygon(centre,q,d.normalized,color,P(j=>d[j]*(j==0?h[j]:i[j])),P(j=>d[j]*(j==1?h[j]:i[j])),P(j=>d[j]*(j==2?h[j]:i[j])));
            }
        }
        public void Sphere(Vector3 centre,Vector3 size,Color color,int slices=16,int rings=10)
        {
            var half=size*.5f;int first=vertices.Count;
            for(int y=0;y<=rings;y++)for(int x=0;x<=slices;x++)
            {
                float t=(float)y/rings*Mathf.PI,a=(float)x/slices*Mathf.PI*2;
                var n=new Vector3(Mathf.Sin(t)*Mathf.Cos(a),Mathf.Cos(t),Mathf.Sin(t)*Mathf.Sin(a));
                Vertex(centre+Vector3.Scale(n,half),new Vector3(n.x/half.x,n.y/half.y,n.z/half.z).normalized,color);
            }
            for(int y=0;y<rings;y++)for(int x=0;x<slices;x++)
            {int a=first+y*(slices+1)+x,b=a+1,c=a+slices+1,d=c+1;Triangle(a,b,d);Triangle(a,d,c);}
        }
        // Smooth sides, flat caps; the axis is local Z.
        public void Cylinder(Vector3 centre,float radius,float length,Color color,Quaternion? rotation=null,int sides=8)
        {
            var q=rotation??Quaternion.identity;int first=vertices.Count;
            for(int ring=0;ring<2;ring++)for(int s=0;s<=sides;s++)
            {
                float a=s*Mathf.PI*2/sides;var radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                Vertex(centre+q*(radial*radius+Vector3.forward*(ring-.5f)*length),q*radial,color);
            }
            for(int s=0;s<sides;s++) {int a=first+s,b=a+1,c=a+sides+1,d=c+1;Triangle(a,b,d);Triangle(a,d,c);}
            foreach(int sign in new[]{-1,1})
            {
                var cap=new Vector3[sides];for(int s=0;s<sides;s++) {float a=s*Mathf.PI*2/sides;cap[s]=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,sign*length*.5f);}
                Polygon(centre,q,Vector3.forward*sign,color,cap);
            }
        }
        public Mesh Build(string name)
        {
            var mesh=new Mesh {name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.SetUVs(0,masks);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
    }
}
