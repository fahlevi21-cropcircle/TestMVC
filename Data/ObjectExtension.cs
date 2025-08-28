namespace TestMVC.Data
{
    public static class ObjectExtension
    {
        //get object value by key
        public static object? GetValue(this object instance, string key)
        {
            var prop = instance.GetType().GetProperty(key);
            return prop?.GetValue(instance, null);
        }
    }
}
