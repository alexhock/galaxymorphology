using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class DataTable
    {
        float[][] data;
        object[] dataObjects;
        int input_dim = -1;
        int data_dim = -1;
        int data_rows = -1;
        public int NODE_STATUS_DIM = -1;
        public const int ACTIVE = 1;
        public const int EMPTY = 0;
        public const int REMOVED = 2;

        public DataTable(int input_dim, List<float[]> positions)
        {
            this.input_dim = positions[0].Length;
            this.data_dim = input_dim + 1;
            this.NODE_STATUS_DIM = this.data_dim - 1; // the last dim position
            this.data_rows = (int) (positions.Count * 1.25);

            data = new float[data_rows][];
            for (int i = 0; i < positions.Count; i++)
            {
                float[] pos = new float[data_dim];
                Array.Copy(positions[i], pos, input_dim);
                pos[this.NODE_STATUS_DIM] = ACTIVE;
                data[i] = pos;
            }

            for (int i = positions.Count; i < data_rows; i++)
                data[i] = new float[data_dim];
        }

        public DataTable(int input_dim, int estimatedSize = 1000)
        {
            this.input_dim = input_dim;
            this.data_dim = input_dim + 1;
            this.NODE_STATUS_DIM = this.data_dim - 1; // the last dim position
            
            this.data_rows = (int)(estimatedSize * 1.5);

            data = new float[data_rows][];
            for(int i=0; i<data_rows;i++)
                data[i] = new float[data_dim];

            dataObjects = new object[data_rows];
        }

        public int NumDimensions
        {
            get { return this.input_dim; }
        }

        public float[][] GetData()
        {
            return this.data;
        }

        public void AddData(int index, float[] position, object o = null)
        {
            if (index >= this.data_rows)
                ExpandData();

            UpdateData(index, position, o);
        }

        public void ReplaceData(List<float[]> positions)
        {
            for (int index = 0; index < positions.Count; index++)
            {
                AddData(index, positions[index]); 
            }
        }

        public void UpdateData(int index, float[] position, object o = null)
        {
            // copy to backup
            for (int i = 0; i < this.input_dim; i++)
                this.data[index][i] = position[i];

            if (o != null)
                dataObjects[index] = o;

            this.data[index][this.NODE_STATUS_DIM] = ACTIVE;
        }

        public float[] GetData(int index)
        {
            float[] position = new float[input_dim];
            for (int i = 0; i < this.input_dim; i++)
                position[i] = this.data[index][i];
            return position;
        }

        public object GetDataObject(int index)
        {
            return this.dataObjects[index];
        }

        public void SetDataObject(int index, object o)
        {
            this.dataObjects[index] = o;
        }

        public void RemoveData(int index)
        {
            this.dataObjects[index] = null;
            this.data[index][NODE_STATUS_DIM] = REMOVED;
        }

        private void ExpandData()
        {
            // expand data
            int new_data_rows = (int)(this.data_rows * 2);

            Console.WriteLine("Expanding: {0} {1}", this.data_rows, new_data_rows);

            float[][] new_data = new float[new_data_rows][];
            object[] newDataObjects = new object[new_data_rows];
            for (int i = 0; i < this.data_rows; i++)
            {
                float[] line = new float[this.data_dim];
                for (int j = 0; j < this.data_dim; j++)
                    line[j] = this.data[i][j];    
                    
                new_data[i] = line;

                newDataObjects[i] = dataObjects[i];
            }

            // initialise remaining floats
            for (int i = this.data_rows; i < new_data_rows; i++)
                new_data[i] = new float[data_dim];
            
            this.data = new_data;
            this.data_rows = new_data_rows;
            this.dataObjects = newDataObjects;
        }


    }
}
