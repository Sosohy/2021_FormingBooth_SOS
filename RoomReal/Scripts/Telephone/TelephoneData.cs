using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TelephoneData
{
    int count;
    int[] numbers;

    public TelephoneData() { }

    public TelephoneData(int count, int[] numbers) {
        this.count = count;
        this.numbers = numbers;
    }

    public int getCount() {
        return count;
    }

    public int[] getNumbers() {
        return numbers;
    }

    public string getString() {
        string s = count + "\n";

        for (int i = 0; i < numbers.Length; i++)
            s += i + " : " + numbers[i] + " / ";

        return s;
    }
}
