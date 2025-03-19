﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ProcuctDemo;
using ProductDemo.Dto;
using ProductDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDemo.Repositories
{
    public interface IProductRepository
    {
        IQueryable<Product> Get();
        Product? Get(int id);
        Product Add(ProductDto item);
        Product? Update(Product item);
        Product? Delete(int id);
    }


    /// <summary>
    /// Repository for product entities
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext applicationDbContext;

        public ProductRepository(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public IQueryable<Product> Get()
        {
            var products = from p in applicationDbContext.Products
                           select new Product() { Id = p.Id, Name = p.Name, Description = p.Description };
            return products;
        }

        public Product? Get(int id) => applicationDbContext.Products.Find(id);

        public Product Add(ProductDto item)
        {
            var result = applicationDbContext.Products.Add(new Product() { Name = item.Name, Description = item.Description });
            applicationDbContext.SaveChanges();
            return result.Entity;
        }

        public Product? Delete(int id)
        {
            Product? result = Get(id);
            if (result != null)
            {
                applicationDbContext.Products.Remove(result);
                applicationDbContext.SaveChanges();
            }
            return result;
        }

        public Product? Update(Product item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Product? result = Get(item.Id);

            if (result != null)
            {

                result.Name = item.Name;
                result.Description = item.Description;

                applicationDbContext.Products.Update(result);   
                applicationDbContext.SaveChanges();
            }

            return result;
        }

    }
}
