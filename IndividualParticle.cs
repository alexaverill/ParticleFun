using Godot;
using System;
using System.Collections.Generic;
/*TODO:
Clean up the calculation of the acceleration modifiers to not use three for loops. 
 */
public class IndividualParticle : Area2D
{
    Random rand = new Random();
    Vector2 newPosition = new Vector2();
    Vector2 velocity = new Vector2();
    Vector2 acceleration;
    float r;
    float maxForce = .03f;
    float maxSpeed = 2f;
    float neighborDistance = 50f; //distance for neightbors that affect velocity
    float minimumSeperation = 30f;//distance to stay away from neighbors.
    float seeAheadDistance = 20f;
    float maxAvoidForce = .05f;
    public override void _Ready()
    {
        //get collider
        //Connect("area_entered",this,"collision");
        float angle = (float)rand.NextDouble() * (3.14159f *2f);
        velocity.x = Mathf.Cos(angle);
        velocity.y = Mathf.Sin(angle);

        
    }

    private void update(){
        newPosition = Position;
        velocity += acceleration;
        newPosition +=velocity;
        SetPosition(newPosition);
        acceleration *=0;
    }
    public void run(List<IndividualParticle> allParticles,List<Obstacle> obstacles,bool breakFlock){
        flock(allParticles,obstacles,breakFlock);
        update();
        borders();
    }
    private void borders(){
        Vector2 newPos = Position;
        if(Position.x<0) newPos.x = 1000;
        if(Position.x > 1000) newPos.x = 0;
        if(Position.y <0) newPos.y = 600;
        if(Position.y > 600) newPos.y = 0;
        SetPosition(newPos);
    }
    void applyToAcceleration(Vector2 force){
        acceleration += force;
    }
    void flock(List<IndividualParticle> neighbors,List<Obstacle> obstacles,bool breakFlock){
        Vector2 seperation = seperate(neighbors);
        Vector2 alignment = align(neighbors);
        Vector2 cohesion = cohese(neighbors);
        Vector2 obstacleAvoidance = avoid(obstacles);
        seperation *= 1.5f;
        alignment *= 1.0f;
        cohesion *= 1.0f;
        if(breakFlock){
            cohesion*=-1;
        }
        applyToAcceleration(obstacleAvoidance);
        applyToAcceleration(cohesion);
        applyToAcceleration(alignment);
        applyToAcceleration(seperation);
    }
    Vector2 avoid(List<Obstacle> obstacles){
        Vector2 viewAhead = Position + velocity.Normalized() * seeAheadDistance;
        Vector2 viewAhead2 = viewAhead *0.5f;
        Vector2 avoidanceForce = new Vector2(0,0);
        Obstacle closest = null;
        foreach(Obstacle o in obstacles){
            if(viewAhead.DistanceTo(o.Position)<=o.radius || viewAhead2.DistanceTo(o.Position)<=o.radius){
                if(closest == null){
                    closest = o;
                }else if(Position.DistanceTo(o.Position)<Position.DistanceTo(closest.Position)){
                    closest = o;
                }
            }
        }
        if(closest !=null){
                avoidanceForce = viewAhead - closest.Position;
                avoidanceForce = avoidanceForce.Normalized();
                avoidanceForce *=maxAvoidForce;
        }
        return avoidanceForce;
    }
    Vector2 seperate(List<IndividualParticle> neighbors){
        
        Vector2 steer = new Vector2();
        int count = 0;
        foreach(IndividualParticle particle in neighbors){
            float distanceTo = Position.DistanceTo(particle.Position);
            if(distanceTo >0 && distanceTo < minimumSeperation){
                    Vector2 difference = Position - particle.Position;
                    difference = difference.Normalized();
                    difference = difference/distanceTo;
                    steer += difference;
                    count ++;
            }
        }
        if(count >0){
            steer = steer/(float)count;
        }
        if(steer.Length()>0){
            steer = steer.Normalized();
            steer *= maxSpeed;
            steer -= velocity;
            steer = steer.Clamped(maxForce);
        }
        return steer;
    }
    Vector2 align(List<IndividualParticle> neighbors){
        Vector2 sum = new Vector2(0,0);
        int count = 0;
        foreach(IndividualParticle particle in neighbors){
            float distanceTo = Position.DistanceTo(particle.Position);
            if(distanceTo >0 && distanceTo < neighborDistance){
                sum += particle.velocity;
                count++;
            }
        }
        if(count >0){
            sum /= (float)count;
            sum = sum.Normalized();
            sum *= maxSpeed;
            Vector2 steer = sum - velocity;
            steer = steer.Clamped(maxForce);
            return steer;
        }
        return new Vector2(0,0);
    }
    Vector2 cohese(List<IndividualParticle> neighbors){
        Vector2 sum = new Vector2(0,0);
        int count = 0;
        foreach(IndividualParticle particle in neighbors){
            float distanceTo = Position.DistanceTo(particle.Position);
            if(distanceTo >0 && distanceTo < neighborDistance){
                sum += particle.Position;
                count++;
            }
        }
        if(count >0){
            sum /=count;
            Vector2 desiredDir = sum - Position;
            desiredDir = desiredDir.Normalized();
            desiredDir *= maxSpeed;
            Vector2 steer = desiredDir - velocity;
            steer = steer.Clamped(maxForce);
            return steer;
        }
        
        return new Vector2(0,0);
    }
}
