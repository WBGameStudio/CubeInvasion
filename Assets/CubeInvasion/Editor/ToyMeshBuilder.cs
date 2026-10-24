using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.ArtTools
{
    // Flat shaded, UV mapped, bevelled meshes; all source remains editable C#.
    public sealed class ToyMeshBuilder
    {
        readonly List<Vector3> vertices=new List<Vector3>(), normals=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int>[] triangles={new List<int>(),new List<int>(),new List<int>()};
        public sealed class PartMesh { public string name;public Vector3 pivot;public Mesh mesh; }
        sealed class PartSource { public string name;public Vector3 pivot;public ToyMeshBuilder builder=new ToyMeshBuilder(); }
        readonly Dictionary<string,PartSource> parts=new Dictionary<string,PartSource>();
        PartSource activePart;
        public void Part(string name,Vector3 pivot)
        {
            if(!parts.TryGetValue(name,out activePart)) { activePart=new PartSource {name=name,pivot=pivot};parts[name]=activePart; }
        }
        public List<PartMesh> BuildParts(string prefix)
        {
            var result=new List<PartMesh>();
            foreach(var part in parts.Values)result.Add(new PartMesh {name=part.name,pivot=part.pivot,mesh=part.builder.Build(prefix+"_"+part.name)});
            return result;
        }
        void Face(Vector3[] points,Vector3 outward,int slot)
        {
            if(activePart!=null)
            {
                var local=new Vector3[points.Length];for(int i=0;i<points.Length;i++)local[i]=points[i]-activePart.pivot;
                activePart.builder.Face(local,outward,slot);
            }
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),outward)<0)System.Array.Reverse(points);
            var normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]).normalized;
            int first=vertices.Count;
            for(int i=0;i<points.Length;i++) {vertices.Add(points[i]);normals.Add(normal);uv.Add(new Vector2(i==1||i==2?1:0,i>=2?1:0));}
            for(int i=1;i<points.Length-1;i++) {triangles[slot].Add(first);triangles[slot].Add(first+i);triangles[slot].Add(first+i+1);}
        }
        static Vector3 Point(int a,float x,int b,float y,int c,float z) {var p=Vector3.zero;p[a]=x;p[b]=y;p[c]=z;return p;}
        public void Box(Vector3 centre,Vector3 size,float bevel=.08f,int slot=0,Quaternion? rotation=null)
        {
            var q=rotation??Quaternion.identity;float r=.5f-Mathf.Clamp(bevel,.001f,.24f);
            System.Action<Vector3[],Vector3> face=(points,n)=>
            {for(int i=0;i<points.Length;i++)points[i]=centre+q*Vector3.Scale(points[i],size);Face(points,q*n,slot);};
            for(int a=0;a<3;a++)for(int sign=-1;sign<=1;sign+=2)
            {
                int b=(a+1)%3,c=(a+2)%3;var normal=Vector3.zero;normal[a]=sign;
                face(new[]{Point(a,sign*.5f,b,-r,c,-r),Point(a,sign*.5f,b,r,c,-r),Point(a,sign*.5f,b,r,c,r),Point(a,sign*.5f,b,-r,c,r)},normal);
            }
            for(int a=0;a<3;a++)for(int b=a+1;b<3;b++)for(int sa=-1;sa<=1;sa+=2)for(int sb=-1;sb<=1;sb+=2)
            {
                int c=3-a-b;var normal=Vector3.zero;normal[a]=sa;normal[b]=sb;
                face(new[]{Point(a,sa*.5f,b,sb*r,c,-r),Point(a,sa*r,b,sb*.5f,c,-r),Point(a,sa*r,b,sb*.5f,c,r),Point(a,sa*.5f,b,sb*r,c,r)},normal);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                face(new[]{new Vector3(x*.5f,y*r,z*r),new Vector3(x*r,y*.5f,z*r),new Vector3(x*r,y*r,z*.5f)},new Vector3(x,y,z));
        }
        public void Drum(Vector3 centre,float radius,float height,int slot=0,int sides=12)
        {
            float[] y={-.5f,-.43f,.43f,.5f},r={.86f,1,1,.86f};
            for(int ring=0;ring<3;ring++)for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Vector3 pa=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),pb=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                Face(new[]{centre+pa*radius*r[ring]+Vector3.up*y[ring]*height,centre+pb*radius*r[ring]+Vector3.up*y[ring]*height,
                    centre+pb*radius*r[ring+1]+Vector3.up*y[ring+1]*height,centre+pa*radius*r[ring+1]+Vector3.up*y[ring+1]*height},pa+pb,slot);
            }
            foreach(int sign in new[]{-1,1})
            {
                var points=new Vector3[sides];
                for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;points[i]=centre+new Vector3(Mathf.Cos(a)*radius*.86f,sign*height*.5f,Mathf.Sin(a)*radius*.86f);}
                Face(points,Vector3.up*sign,slot);
            }
        }
        public void Wedge(Vector3 centre,Vector3 size,int slot=0)
        {
            var points=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(0,0,.5f)};
            var upper=new Vector3[3];var lower=new Vector3[3];
            for(int i=0;i<3;i++){upper[i]=centre+Vector3.Scale(points[i]+Vector3.up*.5f,size);lower[i]=centre+Vector3.Scale(points[i]-Vector3.up*.5f,size);}
            Face(upper,Vector3.up,slot);Face(lower,Vector3.down,slot);
            for(int i=0;i<3;i++){int j=(i+1)%3;var mid=(points[i]+points[j])*.5f;Face(new[]{lower[i],lower[j],upper[j],upper[i]},mid,slot);}
        }
        public Mesh Build(string name)
        {
            var mesh=new Mesh {name=name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.subMeshCount=3;
            for(int i=0;i<3;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateBounds();return mesh;
        }
    }
}
