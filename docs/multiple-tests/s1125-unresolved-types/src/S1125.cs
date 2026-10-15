public class Example
{
    public bool Check(Course course, bool flag, bool? nullableFlag)
    {
        if (course.Enabled == true && !course.Visible.HasValue)
        {
            return nullableFlag == true;
        }

        return flag == true;
    }
}
