/// <summary>
/// 不继承Mono的单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }
    }

    private static T instance;
}