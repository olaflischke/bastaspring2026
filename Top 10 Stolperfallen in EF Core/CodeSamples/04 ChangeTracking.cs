//  PROBLEM
var products = context.Products.ToList(); // Tracking = true

//  LÖSUNG
var products = context.Products
    .AsNoTracking()  // Tracking = false
    .ToList();
