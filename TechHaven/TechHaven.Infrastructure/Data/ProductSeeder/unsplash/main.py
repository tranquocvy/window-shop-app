import requests
import os
from pathlib import Path
import time

# Unsplash API config
UNSPLASH_ACCESS_KEY = "sl7BLqVQAOQnV9Amp2P2Y4WV1x1DCODZnViy-JLXchs"
OUTPUT_DIR = "product_images"

# List of products (copy from your database)
PRODUCTS = [
    {"brand": "Apple", "name": "iPhone 15"},
    {"brand": "Apple", "name": "iPhone 15 Plus"},
    {"brand": "Apple", "name": "iPhone 15 Pro Max"},
    {"brand": "Apple", "name": "iPhone 14"},
    {"brand": "Apple", "name": "iPhone 13"},
    {"brand": "Samsung", "name": "Galaxy A55"},
    {"brand": "Samsung", "name": "Galaxy A35"},
    {"brand": "Samsung", "name": "Galaxy Z Flip 5"},
    {"brand": "Samsung", "name": "Galaxy Z Fold 5"},
    {"brand": "Xiaomi", "name": "14 Ultra"},
    {"brand": "Xiaomi", "name": "13 Pro"},
    {"brand": "Xiaomi", "name": "Redmi Note 12"},
    {"brand": "OnePlus", "name": "11"},
    {"brand": "OnePlus", "name": "Nord CE 4"},
    {"brand": "Google", "name": "Pixel 8 Pro"},
    {"brand": "Google", "name": "Pixel 7"},
    {"brand": "Oppo", "name": "Reno11"},
    {"brand": "Oppo", "name": "A78"},
    {"brand": "Realme", "name": "11 Pro+"},
    {"brand": "Realme", "name": "C55"},
    {"brand": "Vivo", "name": "V29"},
    {"brand": "Vivo", "name": "Y36"},
    {"brand": "Motorola", "name": "Edge 40"},
    {"brand": "Nokia", "name": "G22"},
    {"brand": "Sony", "name": "Xperia 5 V"},
    {"brand": "Honor", "name": "X9b"},
    {"brand": "Infinix", "name": "Hot 30i"},
    {"brand": "Infinix", "name": "Note 30"},
]

def download_image_from_unsplash(brand, product_name, output_dir):
    """Download image from Unsplash"""
    try:
        # Search query
        query = f"{brand} {product_name} phone"
        
        # Search API
        search_url = f"https://api.unsplash.com/search/photos"
        params = {
            "query": query,
            "per_page": 1,
            "client_id": UNSPLASH_ACCESS_KEY
        }
        
        response = requests.get(search_url, params=params)
        response.raise_for_status()
        
        data = response.json()
        
        if not data.get("results"):
            print(f"❌ No image found for: {brand} {product_name}")
            return False
        
        # Get image URL
        image_url = data["results"][0]["urls"]["regular"]
        
        # Download image
        image_response = requests.get(image_url)
        image_response.raise_for_status()
        
        # Save to file
        filename = f"{brand}_{product_name}.jpg".replace(" ", "_")
        filepath = os.path.join(output_dir, filename)
        
        with open(filepath, "wb") as f:
            f.write(image_response.content)
        
        print(f"✅ Downloaded: {filename}")
        return True
        
    except Exception as e:
        print(f"❌ Error downloading {brand} {product_name}: {str(e)}")
        return False

def main():
    # Create output directory
    Path(OUTPUT_DIR).mkdir(exist_ok=True)
    
    print(f"📥 Starting download of {len(PRODUCTS)} product images...")
    print(f"📁 Output directory: {OUTPUT_DIR}")
    print("-" * 60)
    
    success_count = 0
    fail_count = 0
    
    for i, product in enumerate(PRODUCTS, 1):
        print(f"\n[{i}/{len(PRODUCTS)}] Processing: {product['brand']} {product['name']}")
        
        if download_image_from_unsplash(product["brand"], product["name"], OUTPUT_DIR):
            success_count += 1
        else:
            fail_count += 1
        
        # Rate limiting (50 requests/hour for free tier)
        # Sleep 2 seconds between requests
        if i < len(PRODUCTS):
            time.sleep(2)
    
    print("\n" + "=" * 60)
    print(f"✅ Success: {success_count}")
    print(f"❌ Failed: {fail_count}")
    print(f"📁 Images saved to: {os.path.abspath(OUTPUT_DIR)}")

if __name__ == "__main__":
    main()

# Usage:
# 1. pip install requests
# 2. Set your UNSPLASH_ACCESS_KEY
# 3. python download_images.py
# 4. Images will be saved to ./product_images/
# 5. Use BulkImageUploader to upload to Supaba