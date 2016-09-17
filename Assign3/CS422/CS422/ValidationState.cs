/* Colin Phillips
 * CS 422 - Fall 2016
 * Assignment 3
 */

namespace CS422
{
    internal enum ValidationState
    {
        Method = 0,
        Url = 1,
        Version = 2,
        Headers = 3,
        Validated = 4,
        Invalidated = 5
    }
}
