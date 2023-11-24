public enum SceneType
{
    // A single scene does not have any children or parent. Loading into a single scene
    // will ALWAYS go through the loading screen.
    
    // * Transitioning into Single scenes uses the SLOW fade
    // * Transitioning into Single scenes uses Single loading
    Single,
    
    // A parent scene has at least one child. Loading into a Parent scene will ALWAYS
    // go through the loading screen. Upon finished loading the parent scene, the loader
    // will try to load its default Child scene. 
    
    // IMPORTANT: Every parent scene must specify at least one default child scene
    // * Transitioning into Parent scenes uses the SLOW fade
    // * Transitioning into Parent scenes uses Single loading
    Parent, 
    
    // A child scene has exactly one parent. Loading into a Child scene will ALWAYS
    // be a fast transition (no loader callback involved).
    // Loading into a Child scene from a Child scene unloads the last child scene.
    // Loading from a Single scene to a Child scene is not be allowed! (Should always load Parent first)
    
    // * Transitioning into Child scenes uses the FAST fade
    // * Transitioning into Child scenes uses Additive loading
    Child
}