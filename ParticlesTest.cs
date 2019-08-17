using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
public class ParticlesTest : Node2D
{
    [Export]
    public int numParticles = 5;

    List<IndividualParticle> particleList = new List<IndividualParticle>();
    List<Obstacle> obstacles = new List<Obstacle>();
    public override void _Ready()
    {
        // var ob = (Obstacle)GetNode("Obstacle");
        // GD.Print(ob.Position);
        // obstacles.Add(ob);
        var children = GetChildren();
        foreach(Node n in children){

            if(n is Obstacle){
                
                obstacles.Add((Obstacle)n);
            }
        }
        Random rand = new Random();
        var loadedNode = (PackedScene)ResourceLoader.Load("res://IndividualParticle.tscn");
        for(int x =0; x<numParticles; x++){
            var inst = (IndividualParticle)loadedNode.Instance();
            AddChild(inst);
            Vector2 newPosition = new Vector2((float)rand.NextDouble()*1000, (float)rand.NextDouble()*140);
            //GD.Print("x"+newPosition);
            inst.SetPosition(newPosition);
            particleList.Add(inst);
            System.Threading.Thread.Sleep(10);
        }   
    }

 // Called every frame. 'delta' is the elapsed time since the previous frame.
 bool breakFlock = false;
 public override void _Process(float delta)
 {
      if (Input.IsKeyPressed((int)KeyList.Space)){
          breakFlock = true;
      }else{
          breakFlock = false;
      }
     foreach(IndividualParticle n in particleList){
         n.run(particleList,obstacles,breakFlock);
     }
 }
}
