using Terahard.Assessment.Core;
using UnityEngine;
using System;
using ILGPU;
using ILGPU.Runtime;

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

        //A Kernel can only be a static method
        private static void KernelPhase1(Index1D index, ArrayView<Vector2> input, ArrayView<Vector2> output, ArrayView<float> time)
        {
            output[index] = new Vector2 (
                (int)Mathf.Lerp(input[index].x, 0f, time[0] / Core.Application.FRAMES),
                (int)Mathf.Lerp(input[index].y, 1000f, time[0] / Core.Application.FRAMES)
            );
        }

        private static void KernelPhase2(Index1D index, ArrayView<Vector2> input, ArrayView<Vector2> output, ArrayView<float> time)
        {
            output[index] = new Vector2 (
                (int)Mathf.Lerp(input[index].x, 1000f, time[0] / Core.Application.FRAMES),
                (int)Mathf.Lerp(input[index].y, 1000f, time[0] / Core.Application.FRAMES)
            );
        }

        private static void KernelPhase3(Index1D index, ArrayView<Vector2> input, ArrayView<Vector2> output, ArrayView<float> time)
        {
            output[index] = new Vector2 (
                (int)Mathf.Lerp(input[index].x, 1000f, time[0] / Core.Application.FRAMES),
                (int)Mathf.Lerp(input[index].y, 0, time[0] / Core.Application.FRAMES)
            );
        }

        private static void KernelPhase4(Index1D index, ArrayView<Vector2> input, ArrayView<Vector2> output, ArrayView<float> time)
        {
            output[index] = new Vector2 (
                (int)Mathf.Lerp(input[index].x, 0f, time[0] / Core.Application.FRAMES),
                (int)Mathf.Lerp(input[index].y, 0, time[0] / Core.Application.FRAMES)
            );
        }

        /// <summary>
        /// Called by Application
        /// </summary>
        /// <param name="deltaTime">It's always 1.0f to simulate Unity deltaTime</param>
        /// <param name="phase"></param>
        public void Update( float deltaTime, int phase )
        {
            frameCount++;

            //The context is a representation of the computer with it's processing devices
            Context context = Context.CreateDefault();
            Device device;

            //Return CPU device
            //device = context.GetDevice<CPUDevice>(0);

            //Return Cuda device
            //device = context.GetDevice<CudaDevice>(0);

            //Return OpenCL device
            //device = context.GetDevice<CLDevice>(0);

            //Return the most optimal device
            device = context.GetPreferredDevice(preferCPU: false);
            Accelerator accelerator = device.CreateAccelerator(context);

            Vector2[] output = new Vector2[m_Objects.Length];
            Vector2[] input = new Vector2[m_Objects.Length];
            for (int i = 0; i < m_Objects.Length; ++i) { input[i] = m_Objects[i].Position; }

            //Memory pointer for the Kernel operation
            MemoryBuffer1D<float, Stride1D.Dense> deviceTime = accelerator.Allocate1D<float>(new float[1] { deltaTime });
            MemoryBuffer1D<Vector2, Stride1D.Dense> deviceInput = accelerator.Allocate1D<Vector2>(input);
            MemoryBuffer1D<Vector2, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<Vector2>(output);
            deviceInput.CopyFromCPU(input);

            Action<Index1D, ArrayView<Vector2>, ArrayView<Vector2>, ArrayView<float>> kernel;
            if (phase == 1) {
                kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2>, ArrayView<Vector2>, ArrayView<float>>(KernelPhase1);
            }
            else if (phase == 2) {
                kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2>, ArrayView<Vector2>, ArrayView<float>>(KernelPhase2);
            }
            else if (phase == 3) {
                kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2>, ArrayView<Vector2>, ArrayView<float>>(KernelPhase3);
            }
            else {
                kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2>, ArrayView<Vector2>, ArrayView<float>>(KernelPhase4);
            }

            //Repeat the code 'x' times with it's 'device' parameters. Then we synchronize to complete all operations
            kernel((int)deviceOutput.Length, deviceInput.View, deviceOutput.View, deviceTime.View);
            accelerator.Synchronize();

            deviceOutput.CopyToCPU(output);
            for (int i = 0; i < m_Objects.Length; ++i) { m_Objects[i].Position = output[i]; }

            //There is 4 phases, that is why it's multiplied by a quarter (multiplying is faster than dividing)
            //Since this statement only happen at each quarter of the application time, there is no need to multiply Position by 0.25
            //At the end of each phase, all objects will have the same final position. Any member will do for the print value
            if (frameCount % (Core.Application.FRAMES * 0.25) == 0)
            {
                string[] section = new string[4] { "A", "B", "C", "D"};
                Console.WriteLine(section[charIndex] + m_Objects[0].Position * Core.Application.FRAMES);
                charIndex++;
            }

            deviceTime.Dispose();
            deviceInput.Dispose();
            deviceOutput.Dispose();
            accelerator.Dispose();
            context.Dispose();
        }

        public Vector2 GetObjectPosition( int index )
        {
            return m_Objects[ index ].Position;
        }
    }
}