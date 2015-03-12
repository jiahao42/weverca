<?php
/**
 * Class for wrapping the languages
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_languages.php 2373 2011-01-04 15:06:58Z gerundt $
 */

/**
 * Wrapping the languages
 *
 * @package    NOCC
 */
class NOCC_Languages {
    /**
     * Languages
     * @var array
     * @access private
     */
    private $_languages;
    
    /**
     * Default language ID
     * @var string
     * @access private
     */
    private $_defaultLangId;
    
    /**
     * Selected language ID
     * @var string
     * @access private
     */
    private $_selectedLangId;
    
    /**
     * Initialize the languages wrapper
     * @param string $path Languages path (relative)
     * @param string $defaultLangId Default language ID
     */
    public function __construct($path, $defaultLangId = '') {
        $this->_languages = array();
        $this->_defaultLangId = 'en';

	$this->_selectedLangId = 'en';
	
	$this->_languages[$this->_defaultLangId] = $path . $this->_defaultLangId . ".php";
        
	
    }
    
    /**
     * Get the count from the languages
     * @return int Count
     */
    public function count() {
        return count($this->_languages);
    }
    
    /**
     * Exists the language?
     * @param string $langId Language ID
     * @return bool Exists?
     */
    public function exists($langId) {
        if (isset($langId) && is_string($langId) && !empty($langId)) { //if language ID is set...
            $langId = strtolower($langId);
            
            return array_key_exists($langId, $this->_languages);
        }
        return false;
    }
    
    /**
     * Detect the language from the browser...
     * @return string Language ID
     */
    public function detectFromBrowser() {
        return $this->_defaultLangId;
    }
    
    /**
     * Get the default language ID
     * @return string Default language ID
     */
    public function getDefaultLangId() {
        return $this->_defaultLangId;
    }
    
    /**
     * Set the default language ID
     * @param string $langId Default language ID
     * @return bool Successful?
     */
    public function setDefaultLangId($langId) {
        if ($this->exists($langId)) { //if the language exists...
            $this->_defaultLangId = strtolower($langId);
            return true;
        }
        return false;
    }
    
    /**
     * Get the selected language ID
     * @return string Selected language ID
     */
    public function getSelectedLangId() {
        return $this->_defaultLangId;
    }
    
    /**
     * Set the selected language ID
     * @param string $langId Selected language ID
     * @return bool Successful?
     */
    public function setSelectedLangId($langId) {
        return false;
    }
    
    /**
     * Parce the "Accept-Language" header...
     * @param string $acceptLanguageHeader "Accept-Language" header
     * @return array Accepted languages
     * @static
     */
    public static function parseAcceptLanguageHeader($acceptLanguageHeader) {
        $languages = array();
        if (isset($acceptLanguageHeader) && is_string($acceptLanguageHeader) && !empty($acceptLanguageHeader)) { //if the "Accept-Language" header is set...
            $acceptLanguageHeader = strtolower($acceptLanguageHeader);
            $acceptLanguageHeader = str_replace(' ', '', $acceptLanguageHeader);
            $acceptLanguageHeader = str_replace('q=', '', $acceptLanguageHeader);
            
            $langQuality = '1.0';
            $acceptedLanguages = explode(',', $acceptLanguageHeader);
            foreach ($acceptedLanguages as $acceptedLanguage) { //for all accepted languages...
                $tmp = explode(';', $acceptedLanguage);
                
                if (isset($tmp[0]) && !empty($tmp[0])) { //if found language ID...
                    $lang_id = $tmp[0];
                    if (isset($tmp[1]) && !empty($tmp[1])) { //if found language quality...
                        $langQuality = $tmp[1];
                    }
                    $languages[$lang_id] = $langQuality;
                }
            }
        }
        return $languages;
    }
}
?>