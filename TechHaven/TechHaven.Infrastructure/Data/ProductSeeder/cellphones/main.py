import os
import time
import json
import re
import requests
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

# --- CẤU HÌNH ---
BRAND = "tecno" # Tên thương hiệu (dùng để đặt tên folder và url)
BASE_URL = f"https://cellphones.com.vn/mobile/{BRAND}.html"
SAVE_ROOT = f"downloaded_data/{BRAND}" # Folder gốc chứa data

# --- HELPER FUNCTIONS ---

def clean_price(price_text):
    """Chuyển '14.490.000đ' -> 14490000"""
    if not price_text: return 0
    clean = re.sub(r'[^\d]', '', price_text) # Xóa hết ký tự không phải số
    return float(clean) if clean else 0

def clean_number(text):
    """Lấy số từ chuỗi (vd: '6.83 inches' -> 6.83)"""
    if not text: return None
    match = re.search(r"(\d+(\.\d+)?)", text)
    return float(match.group(1)) if match else None

def get_spec_value(driver, spec_name):
    """Tìm giá trị trong bảng thông số kỹ thuật dựa trên tên dòng"""
    try:
        # Tìm dòng tr chứa text spec_name
        xpath = f"//tr[contains(@class, 'technical-content-item') and td[1][contains(text(), '{spec_name}')]]/td[2]"
        element = driver.find_element(By.XPATH, xpath)
        return element.text.strip()
    except:
        return None

def download_image(url, folder, filename):
    """Tải ảnh và lưu vào folder"""
    try:
        # Xử lý URL của CellphoneS để lấy ảnh nét nhất (bỏ qua đoạn crop/resize)
        # URL gốc: .../insecure/rs:fill:358:358/.../plain/https://...jpg
        if "/plain/" in url:
            real_url = url.split("/plain/")[-1]
        else:
            real_url = url
            
        resp = requests.get(real_url, timeout=10)
        if resp.status_code == 200:
            path = os.path.join(folder, filename)
            with open(path, 'wb') as f:
                f.write(resp.content)
            return filename
    except Exception as e:
        print(f"Error downloading {filename}: {e}")
    return None

# --- MAIN CRAWLER ---

def crawl_details():
    # Setup Driver
    options = webdriver.ChromeOptions()
    options.add_argument('--no-sandbox')
    options.add_argument('--disable-dev-shm-usage')
    # options.add_argument('--headless') # Bật dòng này nếu không muốn hiện trình duyệt
    
    driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=options)
    
    try:
        # 1. Lấy danh sách link sản phẩm từ trang danh mục
        print(f"--- Đang truy cập danh mục: {BRAND} ---")
        driver.get(BASE_URL)
        time.sleep(3)
        
        # Scroll để load sản phẩm (Scroll 3 lần demo, muốn lấy hết thì loop nhiều hơn)
        for _ in range(3):
            driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
            time.sleep(2)
            
        # Lấy tất cả link sản phẩm
        link_elements = driver.find_elements(By.CSS_SELECTOR, ".product-info .product__link")
        product_links = [l.get_attribute('href') for l in link_elements]
        # Lọc trùng và lấy 5 sản phẩm đầu tiên để test
        product_links = list(set(product_links))[:5] 
        
        print(f"Tìm thấy {len(product_links)} sản phẩm. Bắt đầu cào chi tiết...")

        # 2. Loop qua từng sản phẩm
        for idx, link in enumerate(product_links):
            try:
                print(f"[{idx+1}/{len(product_links)}] Processing: {link}")
                driver.get(link)
                # Đợi bảng thông số load
                WebDriverWait(driver, 10).until(
                    EC.presence_of_element_located((By.CLASS_NAME, "technical-content"))
                )
                
                # --- A. LẤY METADATA CƠ BẢN ---
                product_name = driver.find_element(By.TAG_NAME, "h1").text.strip()
                
                # Tạo folder riêng cho sản phẩm này
                # Slug hóa tên (vd: "iPhone 16 Pro" -> "iphone-16-pro")
                slug = link.rstrip('/').split('/')[-1].replace('.html', '')
                # Bỏ "dien-thoai-" ở đầu nếu có
                if slug.startswith("dien-thoai-"):
                    slug = slug[len("dien-thoai-"):]
                product_folder = os.path.join(SAVE_ROOT, slug)
                if not os.path.exists(product_folder):
                    os.makedirs(product_folder)

                # Giá bán (xử lý 2 trường hợp: có giá show hoặc chỉ có giá liên hệ)
                try:
                    price_text = driver.find_element(By.CSS_SELECTOR, ".product__price--show").text
                    sell_price = clean_price(price_text)
                except:
                    sell_price = 0

                # --- B. LẤY THÔNG SỐ KỸ THUẬT (Dựa trên HTML bảng user cung cấp) ---
                screen_size_text = get_spec_value(driver, "Kích thước màn hình")
                screen_size = clean_number(screen_size_text)
                
                battery_text = get_spec_value(driver, "Pin")
                battery = int(clean_number(battery_text)) if battery_text else None
                
                storage_text = get_spec_value(driver, "Bộ nhớ trong")
                storage = int(clean_number(storage_text)) if storage_text else None
                
                processor = get_spec_value(driver, "Chipset")
                description = get_spec_value(driver, "Tính năng màn hình") # Tạm lấy cái này làm mô tả ngắn
                
                # --- C. TẢI ẢNH (MAIN + GALLERY) - PHIÊN BẢN ROBUST (SỬA LỖI) ---
                main_img_name = None
                gallery_files = []

                try:
                    # 0. Cuộn nhẹ xuống để kích hoạt lazy load ảnh (quan trọng)
                    driver.execute_script("window.scrollTo(0, 300);")
                    time.sleep(1)

                    # 1. TÌM ẢNH CHÍNH (MAIN IMAGE)
                    # Thử danh sách các selector từ phổ biến đến ít phổ biến
                    main_selectors = [
                        ".box-ksp-gal .swiper-slide-active img",  # Cách cũ (ưu tiên ảnh đang active)
                        ".box-ksp-gal .swiper-slide img",         # Lấy ảnh đầu tiên bất kỳ trong gallery
                        ".gallery-slide img",                     # Class dự phòng
                        ".product-detail__img img",               # Class cho giao diện cũ
                        ".box-detail-product img",                # Class chung
                        "//div[contains(@class, 'gallery-thumbs')]//img" # XPath: Lấy luôn ảnh thumb làm ảnh chính nếu bí
                    ]

                    main_img_url = None
                    for selector in main_selectors:
                        try:
                            if "//" in selector: # Nếu là XPath
                                img_elem = driver.find_element(By.XPATH, selector)
                            else: # Nếu là CSS
                                img_elem = driver.find_element(By.CSS_SELECTOR, selector)
                            
                            main_img_url = img_elem.get_attribute("src")
                            # Nếu lấy được URL và không phải icon svg/base64 rác thì dừng tìm
                            if main_img_url and "http" in main_img_url and "svg" not in main_img_url:
                                print(f"   Found main image with: {selector}")
                                break
                        except:
                            continue
                    
                    if main_img_url:
                        main_img_name = f"{slug}_main.jpg"
                        download_image(main_img_url, product_folder, main_img_name)
                    else:
                        print(f"   [WARNING] Không tìm thấy ảnh chính cho {slug}")

                    # 2. TÌM ẢNH GALLERY (2 ẢNH PHỤ)
                    # Tìm vùng chứa thumbnails
                    try:
                        # Lấy tất cả ảnh trong vùng thumbnail (loại trừ icon tính năng .ksp-thumbs)
                        thumbs = driver.find_elements(By.CSS_SELECTOR, "div[class*='gallery-thumbs'] .swiper-slide:not(.ksp-thumbs) img")
                        
                        # Nếu không tìm thấy bằng class trên, thử tìm mọi ảnh trong swiper
                        if len(thumbs) == 0:
                            thumbs = driver.find_elements(By.CSS_SELECTOR, ".swiper-slide img")

                        count_gal = 0
                        for img_elem in thumbs[1:]:
                            if count_gal >= 2: break # Chỉ lấy 2 ảnh

                            g_url = img_elem.get_attribute("src")
                            # Bỏ qua nếu trùng ảnh chính hoặc URL rác
                            if g_url and g_url != main_img_url and "http" in g_url and "svg" not in g_url:
                                g_name = f"{slug}_gallery_{count_gal+1}.jpg"
                                if download_image(g_url, product_folder, g_name):
                                    gallery_files.append(g_name)
                                    count_gal += 1
                    except Exception as e:
                        print(f"   [WARNING] Lỗi lấy gallery: {e}")

                except Exception as e:
                    print(f"   [ERROR] Lỗi khối xử lý ảnh: {e}")

                # --- D. TẠO DATA ENTITY ---
                entity = {
                    "ProductName": product_name,
                    "BrandName": BRAND.capitalize(),
                    "Color": "Titan", # CellphoneS thường tách màu ra product riêng hoặc phải click chọn màu, tạm hardcode
                    "StorageCapacity": storage,
                    "Processor": processor,
                    "ScreenSize": screen_size,
                    "BatteryCapacity": battery,
                    "ImageUrl": main_img_name, # Chỉ lưu tên file
                    "ImageGalleryJson": json.dumps(gallery_files), # Lưu mảng tên file dạng string JSON
                    "SellPrice": sell_price,
                    "Description": description
                }

                # Lưu file json
                json_path = os.path.join(product_folder, "data.json")
                with open(json_path, 'w', encoding='utf-8') as f:
                    json.dump(entity, f, ensure_ascii=False, indent=4)
                
                print(f"Saved data to {json_path}")
                time.sleep(1) # Nghỉ nhẹ

            except Exception as e:
                print(f"Failed to crawl {link}: {str(e)}")
                continue

    finally:
        driver.quit()
        print("--- CRAWL COMPLETED ---")

if __name__ == "__main__":
    crawl_details()