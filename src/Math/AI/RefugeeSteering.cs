﻿namespace Math.AI
{
    public class RefugeeSteering : SteeringBase
    {
        public RefugeeSteering(Kinetic character, Kinetic target, float maxAcceleration)
            : base(character, target, maxAcceleration)
        {
        }

        public override Vector3 GetLinearAcceleration()
        {
            var direction = Character.Position - Target.Position;
            return direction.Normalized() * MaxAcceleration;
        }
    }
}