using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndulgeamEngine
{

    class ObjectPool<T> where T:new()
    {
        private int capacity;
        public List<T> objects;
        private Queue<T> freeQueue;
        
        public ObjectPool(int capacity,int initSize=0)
        {
            
            if (capacity <= 0||initSize<0||initSize>capacity)
            {
                Debug.Error("对象池初始化出错");
            }
            objects = new List<T>();
            freeQueue = new Queue<T>();
            this.capacity = capacity;
            
            for (int i = 0; i < initSize; i++)
            {
                freeQueue.Enqueue(new T());
            }
        }
        public T Get()
        {
            T obj=default(T);

            if (freeQueue.Count > 0)
            {
                obj = freeQueue.Dequeue();
                objects.Add(obj);


            }
            else if (objects.Count + freeQueue.Count < capacity)
            {
                obj = new T();
                objects.Add(obj);
            }
            else
            {
                obj = default(T);
            }
            return obj;
        }
        public void Delete(T obj)
        {
            freeQueue.Enqueue(obj);
            objects.Remove(obj);
        } 
    }
}
