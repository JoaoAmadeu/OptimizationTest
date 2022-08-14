using Terahard.Assessment.Core;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Terahard.Assessment.Test
{
    class TestContext : IContext
    {
        //Core.Application.OBJECT_COUNT = 100000;
		//Core.Application.FRAMES = 1000;
        
        public string Name => "Joao Amadeu"; //Replace with your name and last name

        //Counter to know exatcly the end of each phase
        private static int frameCount = 1;

        //Index of the last element in the m_Objects array
        private static int index = 0;

        //There is an array of string to represent the name of each phase. This is the index of that array
        private static int charIndex;

        //Array has better performance than List<T>
        //The collection capacity is already know beforehand, so we use it to avoid unnecessary calculus
        private Object[] m_Objects = new Object[Core.Application.OBJECT_COUNT];

        //Since performance is the goal, there is no need for static members
        //private static readonly Vector2 A = new Vector2( 0, 1000 );
        //private static readonly Vector2 B = new Vector2( 1000, 1000 );
        //private static readonly Vector2 C = new Vector2( 1000, 0 );
        //private static readonly Vector2 D = new Vector2( 0, 0 );

        //struct has better performance than class
        public struct Object
        {
            //A field is faster than a propriety
            public Vector2 Position;
            
            public Object( Vector2 position )
            {
                Position = position;
            }

            //The call to this method is replaced entirely by it's content, avoiding the cost of calling it
            /*public void Update( float deltaTime, int phase )
            {
                switch (phase)
                {
                    case 1:
                    {
                        Position = new Vector2 (
                            (int)Mathf.Lerp(Position.x, 0f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(Position.y, 1000f, deltaTime / Core.Application.FRAMES)
                        );
                        break;
                    }

                    case 2:
                    { 
                        Position = new Vector2 (
                            (int)Mathf.Lerp(Position.x, 1000f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(Position.y, 1000f, deltaTime / Core.Application.FRAMES)
                        );
                    break;
                    }

                    case 3: 
                    {
                        Position = new Vector2 (
                            (int)Mathf.Lerp(Position.x, 1000f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(Position.y, 0f, deltaTime / Core.Application.FRAMES)
                        );
                    break;
                    }

                    case 4: 
                    {
                        Position = new Vector2 (
                            (int)Mathf.Lerp(Position.x, 0f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(Position.y, 0f, deltaTime / Core.Application.FRAMES)
                        );
                    break;
                    }
                }
            }*/
        }

        public void CreateObject( Vector2 position )
        {
            Object obj = new Object(position);
            m_Objects[index] = obj;
            index++;
        }

        //The tracking of the removed objects is through the index
        public void RemoveObjects( int count )
        {            
            for (int i = 0; i < count; ++i)
            {
                index--;
            }

            //There is an option to clear the array at the end, if necessary
            //if (index == 0) m_Objects = new Object[0];
        }

        /// <summary>
        /// Called by Application
        /// </summary>
        /// <param name="deltaTime">It's always 1.0f to simulate Unity deltaTime</param>
        /// <param name="phase"></param>
        public void Update( float deltaTime, int phase )
        {
            //Choose the number of threads in parallel
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 10;

            //Start multithreading
            Parallel.For(0, m_Objects.Length, options, (k, state) =>
            {
                Vector2 pos = m_Objects[k].Position;
                switch (phase)
                {
                    case 1:
                    {
                        m_Objects[k].Position = new Vector2(
                            (int)Mathf.Lerp(m_Objects[k].Position.x, 0f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(m_Objects[k].Position.y, 1000f, deltaTime / Core.Application.FRAMES)
                        );
                        break;
                    }

                case 2:
                    {
                        m_Objects[k].Position = new Vector2(
                            (int)Mathf.Lerp(m_Objects[k].Position.x, 1000f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(m_Objects[k].Position.y, 1000f, deltaTime / Core.Application.FRAMES)
                        );
                        break;
                    }

                case 3:
                    {
                        m_Objects[k].Position = new Vector2(
                            (int)Mathf.Lerp(m_Objects[k].Position.x, 1000f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(m_Objects[k].Position.y, 0f, deltaTime / Core.Application.FRAMES)
                        );
                        break;
                    }

                case 4:
                    {
                        m_Objects[k].Position = new Vector2(
                            (int)Mathf.Lerp(m_Objects[k].Position.x, 0f, deltaTime / Core.Application.FRAMES),
                            (int)Mathf.Lerp(m_Objects[k].Position.y, 0f, deltaTime / Core.Application.FRAMES)
                        );
                        break;
                    }
                }
            });

            frameCount++;

            //There is 4 phases, that is why it's multiplied by a quarter (multiplying is faster than dividing)
            //Since this statement only happen at each quarter of the application time, there is no need to multiply Position by 0.25
            //At the end of each phase, all objects will have the same final position. Any member will do for the print value
            if (frameCount % (Core.Application.FRAMES * 0.25) == 0)
            {
                string[] section = new string[4] { "A", "B", "C", "D"};
                Console.WriteLine(section[charIndex] + m_Objects[0].Position * Core.Application.FRAMES);
                charIndex++;
            }
        }

        public Vector2 GetObjectPosition( int index )
        {
            return m_Objects[ index ].Position;
        }
    }
}